using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;
using Kkhay;
using Kkhay.Models;

namespace Kkhay.Tests
{
    public class KkhayTests
    {
        [Fact]
        public void TestClientInitialization()
        {
            using var client = new KkhayClient("kkhay_live_test_123");
            Assert.NotNull(client);
        }

        [Fact]
        public void TestClientThrowsOnEmptyApiKey()
        {
            Assert.Throws<ArgumentException>(() => new KkhayClient(""));
            Assert.Throws<ArgumentException>(() => new KkhayClient("   "));
        }

        [Fact]
        public void TestCreateInvoiceRequestResolvedAmount()
        {
            var req1 = new CreateInvoiceRequest { Amount = 50.0m };
            Assert.Equal(50.0m, req1.ResolvedAmount);
            Assert.Equal("USD", req1.ResolvedCurrency);

            var req2 = new CreateInvoiceRequest { PriceAmount = 99.99m, PriceCurrency = "EUR" };
            Assert.Equal(99.99m, req2.ResolvedAmount);
            Assert.Equal("EUR", req2.ResolvedCurrency);
        }

        [Fact]
        public void TestInvoiceDeserializationGatewayId()
        {
            var json = JsonSerializer.Serialize(new
            {
                id = "01940000-0000-7000-8000-000000000001",
                gatewayId = "gw_019400000000",
                type = "PAYMENT",
                amount = 49.99,
                currency = "USD",
                priceAmount = 49.99,
                priceCurrency = "USD",
                payAmount = "49.99",
                payToken = "USDT",
                payNetwork = "bsc",
                depositAddress = "0x71C...test",
                status = "WAITING",
                hostedUrl = "https://kkhay.com/invoice/01940000-0000-7000-8000-000000000001"
            });

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var invoice = JsonSerializer.Deserialize<Invoice>(json, options);

            Assert.NotNull(invoice);
            Assert.Equal("01940000-0000-7000-8000-000000000001", invoice!.Id);
            Assert.Equal("gw_019400000000", invoice.GatewayId);
            Assert.Equal("WAITING", invoice.Status);
            Assert.Equal("USDT", invoice.PayToken);
        }

        [Fact]
        public void TestWebhookVerification()
        {
            var secret = "whsec_test_secret_123";
            var payload = JsonSerializer.Serialize(new
            {
                @event = "payment.paid",
                invoice_id = "inv_123",
                gateway_id = "gw_test",
                pay_amount = "50.00",
                pay_token = "USDT",
                status = "PAID"
            });

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            // Valid signature
            Assert.True(Webhook.VerifySignature(payload, signature, secret));

            // Tampered payload
            Assert.False(Webhook.VerifySignature(payload + "tampered", signature, secret));

            // Wrong secret
            Assert.False(Webhook.VerifySignature(payload, signature, "wrong_secret"));

            // Empty or null signature
            Assert.False(Webhook.VerifySignature(payload, "", secret));
            Assert.False(Webhook.VerifySignature(payload, null, secret));
        }

        [Fact]
        public void TestWebhookParsing()
        {
            var secret = "whsec_secret_456";
            var payload = JsonSerializer.Serialize(new
            {
                @event = "payment.paid",
                invoice_id = "inv_abc",
                gateway_id = "gw_xyz",
                order_id = "ORD-99",
                price_amount = 50.0m,
                price_currency = "USD",
                pay_amount = "50.00",
                pay_token = "USDT",
                pay_network = "bsc",
                deposit_address = "0x123",
                status = "PAID",
                timestamp = "2026-10-25T00:00:00Z"
            });

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            var evt = Webhook.ParseEvent(payload, signature, secret);
            Assert.Equal("payment.paid", evt.Event);
            Assert.Equal("inv_abc", evt.InvoiceId);
            Assert.Equal("gw_xyz", evt.GatewayId);
            Assert.Equal("ORD-99", evt.OrderId);
            Assert.Equal("USDT", evt.PayToken);
            Assert.Equal("PAID", evt.Status);
        }

        [Fact]
        public void TestWebhookThrowsOnInvalidSignature()
        {
            var secret = "whsec_secret";
            var payload = "{\"event\":\"payment.paid\"}";

            Assert.Throws<SignatureVerificationException>(() =>
                Webhook.ParseEvent(payload, "invalid_signature", secret));
        }
    }
}
