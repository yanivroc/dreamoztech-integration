# API integration reference

This document documents the backend endpoints used by the OvenBites app and provides example payloads for the DreamozTech API (products/pages) and Square payments (create payment + webhook). Use this as a reference for implementers and contributors.

**Environment variables**
- `DREAMOZ_BASE_URL` - Base URL for the DreamozTech API (e.g. `https://api.dreamoztech.example`).
- `DREAMOZ_API_KEY` - Server-side API key for DreamozTech.
- `SQUARE_ACCESS_TOKEN` - Square server-side access token.
- `SQUARE_LOCATION_ID` - Square location id for charges.
- `SQUARE_APPLICATION_ID` - Square app id (used in client SDK).
- `SERVER_URL` - Public server URL for webhooks (e.g., `https://your.domain`).

Keep all these values out of the repo and in environment or secrets management.

-----

## Backend endpoints (example)

These are example backend routes that the app typically provides. The backend proxies and secures calls to DreamozTech and Square.

- `GET /api/products` — return list of products (proxies to DreamozTech `GET /products`).
- `GET /api/products/{slug}` — return single product details by slug or id.
- `POST /admin/products` — create a product (server -> DreamozTech `POST /products`).
- `POST /api/checkout` — create a payment with Square (`paymentsApi.createPayment`).
- `POST /webhooks/square` — receive Square webhook events (payments, refunds).

Each backend endpoint should validate input, check authentication for admin routes, protect secrets, and log errors.

-----

## Example: Fetch product list (backend)

Request to backend (frontend -> server):

```http
GET /api/products
Accept: application/json
```

Backend proxies to DreamozTech:

```http
GET {DREAMOZ_BASE_URL}/products
Authorization: Bearer {DREAMOZ_API_KEY}
```

Example DreamozTech response (truncated):

```json
{
  "data": [
    {
      "id": "prod_123",
      "title": "Sourdough Bread",
      "slug": "sourdough-bread",
      "price": 1299,
      "currency": "USD",
      "shortDescription": "Crispy crust, airy crumb.",
      "images": ["https://cdn.example/imgs/prod_123_1.jpg"],
      "inventory": 12,
      "categories": ["bread", "artisan"]
    }
  ],
  "meta": { "total": 1, "page": 1 }
}
```

Notes:
- `price` is in cents to avoid floating point issues.
- Backend should cache responses where appropriate and respect pagination.

-----

## Example: Product detail (backend)

Request (frontend -> server):

```http
GET /api/products/sourdough-bread
```

Backend -> DreamozTech:

```http
GET {DREAMOZ_BASE_URL}/products/slug/sourdough-bread
Authorization: Bearer {DREAMOZ_API_KEY}
```

Example DreamozTech product object:

```json
{
  "id": "prod_123",
  "title": "Sourdough Bread",
  "slug": "sourdough-bread",
  "description": "A long-fermented sourdough with a crisp crust...",
  "price": 1299,
  "currency": "USD",
  "salePrice": 1099,
  "images": [
    "https://cdn.example/imgs/prod_123_1.jpg",
    "https://cdn.example/imgs/prod_123_2.jpg"
  ],
  "inventory": 12,
  "tags": ["artisan","sourdough"],
  "seo": { "title": "Sourdough Bread - OvenBites", "description": "Buy artisan sourdough" }
}
```

Use this payload to render server-side meta tags and JSON-LD for SEO.

-----

## Example: Create product (admin -> backend -> DreamozTech)

Request to backend (admin only):

```http
POST /admin/products
Content-Type: application/json
Authorization: Bearer <admin-jwt>

{ "title": "Cinnamon Roll", "slug": "cinnamon-roll", "price": 799, "currency":"USD", "inventory": 40 }
```

Backend forwards to DreamozTech:

```http
POST {DREAMOZ_BASE_URL}/products
Authorization: Bearer {DREAMOZ_API_KEY}
Content-Type: application/json

{ "title": "Cinnamon Roll", "slug": "cinnamon-roll", "price": 799, "currency":"USD", "inventory": 40 }
```

Example success response:

```json
{ "id": "prod_456", "title": "Cinnamon Roll", "slug": "cinnamon-roll" }
```

-----

## Square payments: create a payment (backend)

Frontend typically collects card info via Square Web Payments SDK and returns a `sourceId` (a token/nonce). The frontend posts the token and order amount to backend which creates the payment server-side.

Request (frontend -> backend):

```http
POST /api/checkout
Content-Type: application/json

{
  "sourceId": "cnon:card-nonce-ok",
  "idempotencyKey": "uuid-12345",
  "amountCents": 2599,
  "currency": "USD",
  "order": {
    "items": [{ "productId":"prod_123","title":"Sourdough","quantity":2, "unitPrice":1299 }]
  }
}
```

Backend uses Square SDK (Node/.NET) to call `CreatePayment`:

Example request body used by Square SDK (conceptual):

```json
{
  "sourceId": "cnon:card-nonce-ok",
  "idempotencyKey": "uuid-12345",
  "amountMoney": { "amount": 2599, "currency": "USD" },
  "locationId": "<SQUARE_LOCATION_ID>"
}
```

Successful response (simplified):

```json
{
  "payment": {
    "id": "payment_abc",
    "status": "COMPLETED",
    "amountMoney": { "amount": 2599, "currency": "USD" }
  }
}
```

Important:
- Always validate `amountCents` server-side against product prices to avoid client manipulation.
- Use `idempotencyKey` to avoid double charges when retrying.

-----

## Square webhook example (server -> app)

Square sends webhook events to your `SERVER_URL/webhooks/square`. Verify signatures if provided.

Example webhook payload for payment updated:

```json
{
  "merchant_id": "M123",
  "type": "payment.updated",
  "event_id": "evt_001",
  "created_at": "2025-12-05T12:00:00Z",
  "data": {
    "id": "payment_abc",
    "object": {
      "payment": {
        "id": "payment_abc",
        "status": "COMPLETED",
        "amount_money": { "amount": 2599, "currency": "USD" }
      }
    }
  }
}
```

On webhook receipt, update order status in your system and optionally update DreamozTech order status if your integration syncs orders back.

-----

## Error handling and best practices
- Use server-side validation for prices and inventory before charging a customer.
- Log payment attempts and use idempotency keys to make retry behavior safe.
- Rate-limit public endpoints and cache product reads.
- Keep secrets out of source control and rotate keys if leaked.

-----

If you'd like, I can add a small example controller in this repo that implements these routes (`/api/products`, `/api/checkout`, `/webhooks/square`) for C#/.NET to match the project's stack.
