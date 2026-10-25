# K Khay .NET SDK 🔷

**Official C# and .NET SDK for K Khay — The Non-Custodial Crypto Payment Gateway**

[![NuGet version](https://img.shields.io/nuget/v/Kkhay.svg?style=flat-square&color=10b981)](https://www.nuget.org/packages/Kkhay/)
[![CI](https://github.com/boboaungdev/kkhay-dotnet/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/boboaungdev/kkhay-dotnet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-%3E%3D8.0%20%7C%20netstandard2.1-512BD4.svg?style=flat-square)](https://dotnet.microsoft.com/)

Accept cryptocurrency payments across **190+ blockchain networks** and **500+ tokens** (EVM, Bitcoin, Solana, TON, Tron, and XRPL) with **zero custody** and automatic direct-to-wallet settlement in **ASP.NET Core**, Blazor, and .NET applications.

[Gateway Dashboard](https://gateway.kkhay.com) • [API Keys](https://gateway.kkhay.com/dashboard/keys) • [Website](https://kkhay.com)

---

## ⚡ Highlights

- 🛡️ **100% Non-Custodial**: Payments settle directly to your destination blockchain address in seconds. K Khay never holds your private keys or customer funds.
- 🌐 **190+ Networks Supported**:
  - **EVM Networks**: Ethereum, BNB Smart Chain, Polygon, Arbitrum, Base, Optimism, Avalanche, and 180+ more.
  - **UTXO Chains**: Bitcoin (`btc`), Dogecoin (`doge`), Litecoin (`ltc`), Bitcoin Cash (`bch`).
  - **High-Throughput Chains**: Solana (`sol`), TON (`ton`), Tron (`trx`), XRP Ledger (`xrp`).
- 🪙 **500+ Tokens**: Native coins and tokens (USDT, USDC, BTC, ETH, SOL, TON, BNB, DOGE, XRP, TRX, LTC, and all major tokens).
- 🔗 **Branded Hosted Checkouts**: Automatic payment links with dynamic QR codes, live blockchain polling, and wallet deep linking.
- 🔔 **HMAC-SHA256 Cryptographic Webhooks**: Timing-safe verification matching `x-kkhay-signature`.
- 📦 **Dual Target**: Supports `.NET 8.0` and `.NET Standard 2.1` with `System.Text.Json` serialization.

---

## 📦 Installation

Add the official package to your .NET project via the .NET CLI:

```bash
dotnet add package Kkhay
```

Or via Package Manager:

```powershell
Install-Package Kkhay
```

---

## 🚀 Quick Start

### 1. Initialize the Client

Generate your API key in the [K Khay Gateway Dashboard](https://gateway.kkhay.com/dashboard/keys).

```csharp
using Kkhay;
using Kkhay.Models;

using var client = new KkhayClient("kkhay_live_your_api_key_here");
```

#### In ASP.NET Core (Dependency Injection):

```csharp
// Program.cs
builder.Services.AddHttpClient<KkhayClient>((sp, httpClient) =>
{
    var apiKey = builder.Configuration["Kkhay:ApiKey"]!;
    return new KkhayClient(httpClient, apiKey);
});
```

---

### 2. Create a Crypto Payment Invoice

Accept stablecoins, native coins, or let the customer choose their preferred token and chain on checkout:

```csharp
// Example: USDT on BNB Smart Chain (BSC)
var response = await client.CreateInvoiceAsync(new CreateInvoiceRequest
{
    Amount = 49.99m,
    Currency = "USD",
    PayNetwork = "bsc",       // "bsc" | "polygon" | "base" | "arbitrum" | "ethereum" | "solana" | "bitcoin" | "ton"
    PayToken = "USDT",        // "USDT" | "USDC" | "BTC" | "ETH" | "SOL" | "TON"
    OrderId = "ORD-4491",     // Your internal order ID
    Title = "Pro Subscription",
    CustomerEmail = "customer@example.com",
    RedirectUrl = "https://myshop.com/success",
    CancelUrl = "https://myshop.com/cart"
});

Console.WriteLine($"Invoice ID: {response.Invoice.Id}");
Console.WriteLine($"Gateway ID: {response.Invoice.GatewayId}");
Console.WriteLine($"Deposit Address: {response.Invoice.DepositAddress}");
Console.WriteLine($"Hosted Checkout URL: {response.Invoice.HostedUrl}");

// Redirect customer to the checkout page:
// return Results.Redirect(response.Invoice.HostedUrl);
```

---

### 3. Fetch & List Invoices

```csharp
// Retrieve single invoice with blockchain transaction history
var invoiceDetails = await client.GetInvoiceAsync("01940000-0000-7000-8000-000000000001");

Console.WriteLine($"Status: {invoiceDetails.Invoice.Status}"); // WAITING | PAID | EXPIRED | CANCELED
foreach (var payment in invoiceDetails.Payments)
{
    Console.WriteLine($"Received {payment.AmountReceived} on Tx: {payment.TxHash} (Confirms: {payment.Confirmations})");
}

// List invoices with pagination and filters
var list = await client.ListInvoicesAsync(
    page: 1,
    limit: 20,
    status: GatewayInvoiceStatus.Paid
);

Console.WriteLine($"Total Paid Invoices: {list.TotalCount}");
```

---

### 4. Verify Incoming Webhooks (ASP.NET Core Minimal API)

When a customer pays and the blockchain confirms the transaction, K Khay sends a cryptographically signed `POST` webhook to your server:

```csharp
app.MapPost("/api/webhooks/kkhay", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var rawBody = await reader.ReadToEndAsync();
    var signature = request.Headers["x-kkhay-signature"].ToString();
    var webhookSecret = Environment.GetEnvironmentVariable("KKHAY_WEBHOOK_SECRET")!;

    try
    {
        var evt = Webhook.ParseEvent(rawBody, signature, webhookSecret);

        if (evt.Event == "payment.paid")
        {
            Console.WriteLine($"Payment confirmed for Order: {evt.OrderId}");
            Console.WriteLine($"Tx Hash: {evt.TxHash}");
            Console.WriteLine($"Received: {evt.PayAmount} {evt.PayToken} on {evt.PayNetwork}");
            // Fulfill customer order in database
        }

        return Results.Ok(new { ok = true });
    }
    catch (SignatureVerificationException)
    {
        return Results.Unauthorized();
    }
});
```

---

## 🌐 Supported Networks & Families

| Network Family | Popular Chains | Supported Tokens |
| :--- | :--- | :--- |
| **EVM (180+ Chains)** | Ethereum, BNB Smart Chain, Polygon, Arbitrum, Base, Optimism, Avalanche | USDT, USDC, ETH, BNB, POL, AVAX, DAI, ERC-20s |
| **UTXO** | Bitcoin, Dogecoin, Litecoin, Bitcoin Cash | BTC, DOGE, LTC, BCH |
| **Solana** | Solana Mainnet | SOL, USDT, USDC |
| **TON** | The Open Network | TON, USDT |
| **Tron (TVM)** | Tron Mainnet | TRX, USDT |
| **XRPL** | XRP Ledger | XRP |

---

## 📄 License

MIT © [K Khay](https://kkhay.com)
