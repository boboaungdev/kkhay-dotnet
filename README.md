# K Khay .NET SDK 🔷

Official C# and .NET SDK for the **[K Khay Sovereign Crypto Payment Gateway](https://kkhay.com)**.

Accept non-custodial and custodial crypto payments (USDT, USDC, BNB, ETH on BSC, Polygon, Arbitrum, Base, Ethereum) in **ASP.NET Core**, Blazor, and .NET applications.

---

## 📦 Installation

```bash
dotnet add package Kkhay
```

*(Supports .NET 8, .NET 7, .NET 6, and .NET Standard 2.1)*

---

## ⚡ Quick Start

```csharp
using Kkhay;
using Kkhay.Models;

using var client = new KkhayClient("kkhay_live_your_api_key_here");

var response = await client.CreateInvoiceAsync(new CreateInvoiceRequest
{
    PriceAmount = 49.99m,
    PriceCurrency = "USD",
    PayNetwork = "bsc",
    PayToken = "USDT",
    OrderId = "ORD-4491",
    Title = "Enterprise License",
    CustomerEmail = "customer@example.com",
    RedirectUrl = "https://myshop.com/success"
});

Console.WriteLine($"Invoice ID: {response.Invoice.Id}");
Console.WriteLine($"Pay URL: {response.Invoice.HostedUrl}");
```

---

## 🔐 Webhook / IPN Verification (ASP.NET Core Minimal API)

```csharp
app.MapPost("/api/webhooks/kkhay", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var rawBody = await reader.ReadToEndAsync();
    var signature = request.Headers["x-kkhay-signature"].ToString();
    var ipnSecret = Environment.GetEnvironmentVariable("KKHAY_IPN_SECRET")!;

    try
    {
        var evt = Webhook.ParseEvent(rawBody, signature, ipnSecret);

        if (evt.Event == "payment.finished")
        {
            Console.WriteLine($"Order {evt.OrderId} paid via Tx: {evt.TxHash}");
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

## 📄 License

MIT © [K Khay](https://kkhay.com)

