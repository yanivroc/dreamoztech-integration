# Dreamoz Razor Pages App

Short description: A Razor Pages web application targeting .NET 8 that integrates with the Dreamoz API to list products and allow customers to add items to a cart and checkout. The live application is available at: https://dreamoz.com.au/shop

This README lives at the repository root (next to the solution `.sln`) and explains how the app connects to the Dreamoz API, how product and cart flows work, and how to run and test the application locally.

## Key points / overview

- Framework: .NET 8, Razor Pages.
- Purpose: Fetch products from the Dreamoz API, display them in the shop UI, allow adding to a cart, and complete a checkout (supports test-mode payments).
- Live site: `https://dreamoz.com.au/shop`
- Follow repository rules: Observe `.editorconfig` and `CONTRIBUTING.md` for formatting, naming, and commit guidance.

## How the app integrates with the Dreamoz API

High-level behavior:
1. On page load the app requests product data from the Dreamoz API (public or authenticated endpoint as configured).
2. Responses are deserialized to strongly typed DTOs and cached (short-term) to reduce API calls.
3. Individual product pages show details and an "Add to cart" action which stores cart state in session (server-side) or in a client cookie depending on configuration.
4. Checkout collects cart contents, computes totals and tax/shipping (if required) and forwards payment information to the payment gateway used by the site (configured via app settings).

Typical integration patterns used in the project:
- A typed `DreamozApiClient` or service registered in DI (look for `Services` or `Clients` in the project) handles:
  - Base URL configuration (e.g., `https://dreamoz.com.au/api/...`).
  - Optional API key or bearer token injected from `appsettings.*` or environment variables.
  - Retry/backoff and basic caching for product lists.
- Razor Pages call the service from page handlers (`OnGet`, `OnPost`) and map models to view models for rendering.

Configuration settings are found in `appsettings.json` (and `appsettings.Development.json`) and may include:
- `DreamozApi:BaseUrl`
- `DreamozApi:ApiKey` (if required)
- Payment gateway keys / sandbox mode flags

Always prefer secrets and environment variables for keys in development and production.

## Product & cart flow (user experience)

1. Shop page fetches product list from `Dreamoz` API and renders product cards.
2. Click a product to view details; select options (size, color) if applicable.
3. Click `Add to cart` — cart stored in session/cookie and reflected in the header/cart page.
4. Navigate to cart page to update quantities or remove items.
5. Proceed to checkout — payment form uses the configured gateway.

Implementation notes:
- Cart state is typically implemented as a small server-side model persisted to the session (or as an encrypted cookie) so Razor Pages handlers can mutate it without a full client-side store.
- Keep product DTOs immutable and map to UI view models to avoid leaking API contracts into view code.

## Testing checkout with test cards

- The app should be configured to use the payment gateway's test/sandbox environment in development. Check `appsettings.Development.json` or an environment flag `Payment:UseSandbox = true`.
- If the site uses Stripe (example), a common test card is:
  - Card number: `4242 4242 4242 4242`
  - Expiry: any valid future date
  - CVC: any 3 digits
- Use the test credentials provided by the payment provider — never use real card numbers in development or public repositories.
- After switching to sandbox/test mode you can perform full checkout flows without charging real cards.

## Run locally

Requirements:
- .NET 8 SDK
- Visual Studio 2022 (updated) or VS Code with C# extension

Quick start from the solution root:
1. Restore and build:
2. Run the web project (replace path with your web project csproj if different):
3. Open the provided URL (usually `https://localhost:5001` or as shown in the console). The shop will attempt to fetch products from `Dreamoz` API using configured base URL.

Tip: Use `appsettings.Development.json` or environment variables to point to the sandbox/test API endpoints and payment gateway keys.

## Environment & secrets

- Use `dotnet user-secrets` during development:
- In production, provision secrets via environment variables, a secret store, or your deployment platform.

## Troubleshooting

- "Products not loading": verify `DreamozApi:BaseUrl` and any API keys; check console logs and network tab.
- "Payment fails in dev": ensure gateway is in sandbox/test mode and use provider test cards.
- CORS: if running frontend on a different host during development, confirm the API allows requests from your origin.