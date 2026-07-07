using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace WishlistApi.Controllers
{
    public class ScrapeRequestDto
    {
        public string? url { get; set; }
    }

    [ApiController]
    [EnableRateLimiting("ScrapeLimit")]
    public class ScraperController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ScraperController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("api/scrape")]
        [Authorize]
        public async Task<IActionResult> ScrapeUrl([FromBody] ScrapeRequestDto request)
        {
            var url = request?.url;
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(new { message = "URL cannot be empty." });
            }

            try
            {
                using var httpClient = new HttpClient();
                string html;
                var scraperApiKey = Environment.GetEnvironmentVariable("SCRAPER_API_KEY") ?? _config["ScraperApi:ApiKey"];
                
                Console.WriteLine($"[Scraper] ScrapeUrl called. Using ScraperAPI: {!string.IsNullOrEmpty(scraperApiKey)}");

                if (!string.IsNullOrEmpty(scraperApiKey))
                {
                    var scraperUrl = $"https://api.scraperapi.com/?api_key={scraperApiKey}&url={Uri.EscapeDataString(url)}&country_code=tr";
                    html = await httpClient.GetStringAsync(scraperUrl);
                }
                else
                {
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
                    httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
                    httpClient.DefaultRequestHeaders.Add("Referer", "https://www.trendyol.com/");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Google Chrome\";v=\"123\", \"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"123\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"macOS\"");
                    httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
                    httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
                    httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
                    httpClient.DefaultRequestHeaders.Add("sec-fetch-user", "?1");
                    httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    
                    html = await httpClient.GetStringAsync(url);
                }
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

              
                string? title = null;
                string? imageUrl = null;
                string? description = null;
                decimal? price = null;

                // 1. Try Meta Tags first for images (often more accurate for variants)
                var ogImageNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                if (ogImageNode != null) imageUrl = ogImageNode.GetAttributeValue("content", null);

                var ogTitleNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
                if (ogTitleNode != null) title = System.Net.WebUtility.HtmlDecode(ogTitleNode.GetAttributeValue("content", null));

                var ogDescNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
                if (ogDescNode != null) description = System.Net.WebUtility.HtmlDecode(ogDescNode.GetAttributeValue("content", null));

                // 2. Try JSON-LD for Price and deeper info
                var scriptNodes = htmlDoc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
                if (scriptNodes != null)
                {
                    foreach (var node in scriptNodes)
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(node.InnerText);
                            var root = doc.RootElement;
                            if (root.TryGetProperty("@type", out var typeElement))
                            {
                                var typeStr = typeElement.GetString();

                                if (typeStr == "Product")
                                {
                                    if (string.IsNullOrEmpty(title) && root.TryGetProperty("name", out var nameProp)) title = nameProp.GetString();
                                    if (string.IsNullOrEmpty(description) && root.TryGetProperty("description", out var descProp)) description = descProp.GetString();
                                    
                                    if (string.IsNullOrEmpty(imageUrl) && root.TryGetProperty("image", out var imgProp))
                                    {
                                        if (imgProp.ValueKind == JsonValueKind.String)
                                            imageUrl = imgProp.GetString();
                                        else if (imgProp.ValueKind == JsonValueKind.Object && imgProp.TryGetProperty("contentUrl", out var cuProp))
                                            imageUrl = cuProp.ValueKind == JsonValueKind.Array && cuProp.GetArrayLength() > 0
                                                ? cuProp[0].GetString() : cuProp.GetString();
                                        else if (imgProp.ValueKind == JsonValueKind.Array && imgProp.GetArrayLength() > 0)
                                            imageUrl = imgProp[0].GetString();
                                    }

                                    if (root.TryGetProperty("offers", out var offersProp))
                                    {
                                        var offerToUse = offersProp.ValueKind == JsonValueKind.Array && offersProp.GetArrayLength() > 0
                                            ? offersProp[0] : offersProp;
                                        ExtractPrice(offerToUse, ref price);
                                    }
                                    break;
                                }

                                if (typeStr == "ProductGroup")
                                {
                                    if (string.IsNullOrEmpty(title) && root.TryGetProperty("name", out var nameProp)) title = nameProp.GetString();
                                    
                                    if (root.TryGetProperty("hasVariant", out var variantsProp) &&
                                        variantsProp.ValueKind == JsonValueKind.Array &&
                                        variantsProp.GetArrayLength() > 0)
                                    {
                                        // Try to find variant in URL
                                        var selectedVariant = variantsProp[0];
                                        foreach (var variant in variantsProp.EnumerateArray())
                                        {
                                            if (variant.TryGetProperty("sku", out var skuProp) && url.Contains(skuProp.GetString() ?? ""))
                                            {
                                                selectedVariant = variant;
                                                break;
                                            }
                                        }

                                        if (string.IsNullOrEmpty(imageUrl) && selectedVariant.TryGetProperty("image", out var vImgProp))
                                        {
                                            if (vImgProp.ValueKind == JsonValueKind.String)
                                                imageUrl = vImgProp.GetString();
                                        }

                                        if (selectedVariant.TryGetProperty("offers", out var vOffersProp))
                                        {
                                            var offerToUse = vOffersProp.ValueKind == JsonValueKind.Array && vOffersProp.GetArrayLength() > 0
                                                ? vOffersProp[0] : vOffersProp;
                                            ExtractPrice(offerToUse, ref price);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        catch { /* Ignore parse errors */ }
                    }
                }

            
                if (string.IsNullOrEmpty(title))
                {
                    var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
                    if (titleNode != null) title = System.Net.WebUtility.HtmlDecode(titleNode.InnerText);
                }

                if (!string.IsNullOrEmpty(title))
                {
                    title = title.Replace(" - Trendyol", "")
                                 .Replace(" : Amazon.com.tr", "")
                                 .Replace(" : Amazon.com", "")
                                 .Replace("Amazon.com.tr: ", "")
                                 .Replace("Amazon.com: ", "");
                    int dashIndex = title.IndexOf(" - Fiyatı");
                    if (dashIndex > 0) title = title.Substring(0, dashIndex).Trim();
                }
                
                if (!string.IsNullOrEmpty(description))
                {
                    description = description.Replace(" - Trendyol", "");
                    int promoIndex = description.IndexOf(" yorumlarını inceleyin");
                    if (promoIndex > 0) description = description.Substring(0, promoIndex).Trim();
                }

                // Fallback image parser if og:image or json-ld failed
                if (string.IsNullOrEmpty(imageUrl))
                {
                    var landingImageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[@id='landingImage']")
                                           ?? htmlDoc.DocumentNode.SelectSingleNode("//img[@id='imgBlkFront']")
                                           ?? htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']")
                                           ?? htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'a-dynamic-image')]");
                    if (landingImageNode != null)
                    {
                        imageUrl = landingImageNode.GetAttributeValue("src", null) 
                                   ?? landingImageNode.GetAttributeValue("content", null);
                    }
                }

                var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='product:price:amount']");
                if (priceNode != null)
                {
                    var priceStr = priceNode.GetAttributeValue("content", null);
                    if (decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedPrice))
                    {
                        price = parsedPrice;
                    }
                }

                if (price == null)
                {
                    var trendyolPriceNode = htmlDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'prc-dsc')]") 
                                            ?? htmlDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'prc-slg')]");
                    
                    if (trendyolPriceNode != null)
                    {
                        price = CleanAndParsePrice(trendyolPriceNode.InnerText);
                    }
                }

                if (price == null)
                {
                    var amazonPriceNode = htmlDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'a-price')]//span[@class='a-offscreen']")
                                          ?? htmlDoc.DocumentNode.SelectSingleNode("//span[@id='priceblock_ourprice']")
                                          ?? htmlDoc.DocumentNode.SelectSingleNode("//span[@id='priceblock_dealprice']")
                                          ?? htmlDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'a-offscreen')]");
                    if (amazonPriceNode != null)
                    {
                        price = CleanAndParsePrice(amazonPriceNode.InnerText);
                    }
                }

                return Ok(new
                {
                    data = new {
                        title = title?.Trim(),
                        image = imageUrl?.Trim(),
                        description = description?.Trim(),
                        price = price,
                        sourceUrl = url
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to scrape URL: " + ex.Message });
            }
        }

        private static void ExtractPrice(JsonElement offerElement, ref decimal? price)
        {
            if (offerElement.ValueKind == JsonValueKind.Object && offerElement.TryGetProperty("price", out var priceProp))
            {
                var priceStr = priceProp.ValueKind == JsonValueKind.Number
                    ? priceProp.GetRawText()
                    : priceProp.GetString();

                if (decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                {
                    price = parsed;
                }
            }
        }

        private static decimal? CleanAndParsePrice(string priceText)
        {
            if (string.IsNullOrEmpty(priceText)) return null;

            priceText = priceText.Replace("TL", "")
                                 .Replace("TRY", "")
                                 .Replace("$", "")
                                 .Replace("€", "")
                                 .Replace("£", "")
                                 .Replace(" ", "")
                                 .Replace("\u00a0", "")
                                 .Trim();

            if (priceText.Contains(",") && priceText.Contains("."))
            {
                if (priceText.IndexOf(",") > priceText.IndexOf("."))
                {
                    priceText = priceText.Replace(".", "").Replace(",", ".");
                }
                else
                {
                    priceText = priceText.Replace(",", "");
                }
            }
            else if (priceText.Contains(","))
            {
                priceText = priceText.Replace(",", ".");
            }

            if (decimal.TryParse(priceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedPrice))
            {
                return parsedPrice;
            }
            return null;
        }
    }
}
