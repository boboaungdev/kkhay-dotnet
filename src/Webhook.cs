using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Kkhay.Models;

namespace Kkhay
{
    public static class Webhook
    {
        /// <summary>
        /// Verifies the cryptographic HMAC-SHA256 signature sent with incoming K Khay IPN webhooks.
        /// </summary>
        public static bool VerifySignature(string payload, string? signature, string ipnSecret)
        {
            if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(ipnSecret))
                return false;

            try
            {
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ipnSecret.Trim()));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var expectedHex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                var expectedBytes = Encoding.UTF8.GetBytes(expectedHex);
                var actualBytes = Encoding.UTF8.GetBytes(signature.Trim().ToLowerInvariant());

                if (expectedBytes.Length != actualBytes.Length)
                    return false;

                return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates signature and parses the raw JSON payload into a typed WebhookEvent.
        /// </summary>
        public static WebhookEvent ParseEvent(string rawBody, string? signature, string ipnSecret)
        {
            if (!VerifySignature(rawBody, signature, ipnSecret))
            {
                throw new SignatureVerificationException("Invalid K Khay webhook signature: Request rejected.");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var evt = JsonSerializer.Deserialize<WebhookEvent>(rawBody, options);

            return evt ?? throw new InvalidOperationException("Failed to deserialize webhook JSON.");
        }
    }
}

