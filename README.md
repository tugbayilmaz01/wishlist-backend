# Wishtra — Backend API

**Wishtra** is a modern wishlist management app. This repository contains the REST API built with .NET 9 and PostgreSQL.

🌐 **Live App**: [www.wishtra.com](https://www.wishtra.com)

---

## ✨ Features

- 🔐 **JWT Authentication** — Secure token-based auth with long-lived sessions
- 🔑 **Google OAuth** — Social login via Google ID token verification
- 📋 **Wishlist CRUD** — Full create, read, update, delete for wishlists and products
- 🤖 **Web Scraper** — Auto-extract product info (title, image, price) from URLs
  - Powered by **ScraperAPI** for bypassing bot protection (Cloudflare, etc.)
  - Supports Trendyol, Amazon, and generic Open Graph sites
  - Automatic retry logic with geo-block detection
- 📧 **Email Service** — Password reset emails via **Resend** HTTP API
- 🔗 **Sharing & Collaboration** — Shareable tokens and collaborator system
- 🛡️ **Rate Limiting** — Built-in scraper rate limiter to prevent abuse

---

## 🛠️ Tech Stack

- **Framework**: ASP.NET Core 9
- **ORM**: Entity Framework Core 9
- **Database**: PostgreSQL
- **Auth**: JWT Bearer + Google APIs Auth
- **Scraping**: HtmlAgilityPack + ScraperAPI
- **Email**: Resend HTTP API
- **Deployment**: Render (Docker)

---

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- PostgreSQL database

### Installation

```bash
# Clone the repository
git clone https://github.com/tugbayilmaz01/wishlist-backend.git
cd wishlist-backend/WishlistApi

# Set up User Secrets (for local development)
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "your_jwt_secret"
dotnet user-secrets set "Email:ResendApiKey" "re_your_resend_api_key"
dotnet user-secrets set "ScraperApi:ApiKey" "your_scraperapi_key"

# Apply database migrations
dotnet ef database update

# Run the API
dotnet run
```

API will be available at `http://localhost:5000`.

### Environment Variables (Production)

Set the following in your deployment platform (e.g., Render):

```
JWT_SECRET=
GOOGLE_CLIENT_ID=
RESEND_API_KEY=
SCRAPER_API_KEY=
DATABASE_URL=
FRONTEND_URL=
```

---

## 📁 Project Structure

```
WishlistApi/
├── controllers/       # API endpoint controllers
│   ├── UsersController.cs      # Auth (register, login, Google OAuth)
│   ├── WishlistsController.cs  # Wishlist & product CRUD
│   └── ScraperController.cs    # Product URL scraper
├── models/            # Entity models
├── data/              # EF Core DbContext
├── Services/          # Business logic (Email, JWT)
├── Migrations/        # EF Core database migrations
├── Program.cs         # App configuration & middleware
└── appsettings.json   # App settings (no secrets)
```

---

## 📡 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/users/register` | Register new user |
| POST | `/api/users/login` | Login with email/password |
| POST | `/api/users/social-login` | Login with Google token |
| POST | `/api/users/forgot-password` | Send password reset email |
| POST | `/api/users/reset-password` | Reset password with token |
| GET | `/api/wishlists` | Get all user wishlists |
| POST | `/api/wishlists` | Create wishlist |
| PUT | `/api/wishlists/{id}` | Update wishlist |
| DELETE | `/api/wishlists/{id}` | Delete wishlist |
| POST | `/api/wishlists/{id}/products` | Add product to wishlist |
| POST | `/api/scrape` | Scrape product info from URL |

---

## 🔗 Related

- **Frontend**: [wishlist-frontend](https://github.com/tugbayilmaz01/wishlist-frontend) — Next.js 15 web app

---

## 📄 License

MIT
