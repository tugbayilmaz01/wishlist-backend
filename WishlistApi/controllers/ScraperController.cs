using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WishlistApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScraperController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> ScrapeUrl([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("URL cannot be empty.");
            }

            try
            {
                using var httpClient = new HttpClient();
          
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
                
                var html = await httpClient.GetStringAsync(url);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

              
                string title = null;
                string imageUrl = null;
                string description = null;
                decimal? price = null;

               
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
                                    if (root.TryGetProperty("name", out var nameProp)) title = nameProp.GetString();
                                    if (root.TryGetProperty("description", out var descProp)) description = descProp.GetString();
                                    
                                    if (root.TryGetProperty("image", out var imgProp))
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
                                    if (root.TryGetProperty("name", out var nameProp)) title = nameProp.GetString();
                                    if (root.TryGetProperty("description", out var descProp)) description = descProp.GetString();

                                   
                                    if (root.TryGetProperty("hasVariant", out var variantsProp) &&
                                        variantsProp.ValueKind == JsonValueKind.Array &&
                                        variantsProp.GetArrayLength() > 0)
                                    {
                                        var firstVariant = variantsProp[0];

                                     
                                        if (imageUrl == null && firstVariant.TryGetProperty("image", out var vImgProp))
                                        {
                                            if (vImgProp.ValueKind == JsonValueKind.String)
                                                imageUrl = vImgProp.GetString();
                                        }

                                        if (firstVariant.TryGetProperty("offers", out var vOffersProp))
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
                        catch { /* Ignore parse errors on individual scripts */ }
                    }
                }

        
                if (string.IsNullOrEmpty(title))
                {
                    var ogTitleNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
                    if (ogTitleNode != null) title = System.Net.WebUtility.HtmlDecode(ogTitleNode.GetAttributeValue("content", null));
                }

                if (string.IsNullOrEmpty(imageUrl))
                {
                    var ogImageNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                    if (ogImageNode != null) imageUrl = ogImageNode.GetAttributeValue("content", null);
                }

                var ogDescNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
                if (ogDescNode != null) description = System.Net.WebUtility.HtmlDecode(ogDescNode.GetAttributeValue("content", null));

            
                if (string.IsNullOrEmpty(title))
                {
                    var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
                    if (titleNode != null) title = System.Net.WebUtility.HtmlDecode(titleNode.InnerText);
                }

               
                if (!string.IsNullOrEmpty(title))
                {
                    title = title.Replace(" - Trendyol", "");
                    int dashIndex = title.IndexOf(" - Fiyatı");
                    if (dashIndex > 0) title = title.Substring(0, dashIndex).Trim();
                }
                
                if (!string.IsNullOrEmpty(description))
                {
                    description = description.Replace(" - Trendyol", "");
                    int promoIndex = description.IndexOf(" yorumlarını inceleyin");
                    if (promoIndex > 0) description = description.Substring(0, promoIndex).Trim();
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
                        var priceText = trendyolPriceNode.InnerText.Replace("TL", "").Trim();
                        priceText = priceText.Replace(".", "").Replace(",", ".");
                        if (decimal.TryParse(priceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedPrice))
                        {
                            price = parsedPrice;
                        }
                    }
                }

                return Ok(new
                {
                    Title = title?.Trim(),
                    ImageUrl = imageUrl?.Trim(),
                    Description = description?.Trim(),
                    Price = price,
                    SourceUrl = url
                });
            }
            catch (Exception ex)
            {
                return Ok(new { Error = "Failed to scrape URL: " + ex.Message });
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
    }
}
