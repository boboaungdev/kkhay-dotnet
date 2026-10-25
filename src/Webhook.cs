using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Kkhay.Models;

namespace Kkhay
{
    /// <summary>
    /// Cryptographic helper for verifying incoming K Khay IPN webhooks.
    /// </summary>
    public static class Webhook
    {
        /// <summary>
        /// Verifies the HMAC-SHA256 signature sent with incoming K Khay IPN webhooks.
        /// </summary>
        /// <param name="payload">The raw request body string</param>
        /// <param name="signature">The signature from the 'x-kkhay-signature' header</param>
        /// <param name="webhookSecret">Your gateway Webhook Secret Key</param>
        /// <returns>True if the signature is authentic and untampered</returns>
        public static bool VerifySignature(string payload, string? signature, string webhookSecret)
        {
            if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(webhookSecret))
                return false;

            try
            {
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret.Trim()));
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
        /// Throws <see cref="SignatureVerificationException"/> if the signature does not match.
        /// </summary>
        public static WebhookEvent ParseEvent(string rawBody, string? signature, string webhookSecret)
        {
            if (!VerifySignature(rawBody, signature, webhookSecret))
            {
                throw new SignatureVerificationException("Invalid or untrusted K Khay webhook signature (x-kkhay-signature mismatch).");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var evt = JsonSerializer.Deserialize<WebhookEvent>(rawBody, options);

            return evt ?? throw new InvalidOperationException("Failed to deserialize K Khay webhook JSON payload.");
        }
    }
}
