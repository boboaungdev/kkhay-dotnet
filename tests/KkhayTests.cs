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
        public void TestWebhookVerification()
        {
            var secret = "whsec_test_secret_123";
            var payload = JsonSerializer.Serialize(new
            {
                event_name = "payment.finished",
                invoice_id = "inv_123",
                pay_amount = "50.00",
                pay_token = "USDT"
            });

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            // Valid
            Assert.True(Webhook.VerifySignature(payload, signature, secret));

            // Tampered
            Assert.False(Webhook.VerifySignature(payload + "tampered", signature, secret));

            // Wrong secret
            Assert.False(Webhook.VerifySignature(payload, signature, "wrong_secret"));

            // Empty sig
            Assert.False(Webhook.VerifySignature(payload, "", secret));
        }

        [Fact]
        public void TestWebhookParsing()
        {
            var secret = "whsec_secret";
            var payload = JsonSerializer.Serialize(new
            {
                @event = "payment.finished",
                invoice_id = "inv_abc",
                order_id = "ORD-1",
                price_amount = 50.0m,
                price_currency = "USD",
                pay_amount = "50.00",
                pay_token = "USDT",
                pay_network = "bsc",
                deposit_address = "0x123",
                status = "paid",
                timestamp = "2026-10-07T00:00:00Z"
            });

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            var evt = Webhook.ParseEvent(payload, signature, secret);
            Assert.Equal("payment.finished", evt.Event);
            Assert.Equal("inv_abc", evt.InvoiceId);
            Assert.Equal("USDT", evt.PayToken);
        }
    }
}

