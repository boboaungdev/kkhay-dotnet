using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kkhay.Models;

namespace Kkhay
{
    /// <summary>
    /// Official C# and .NET client for the K Khay Non-Custodial Crypto Payment Gateway.
    /// Supports 190+ blockchain networks and 500+ cryptocurrencies with zero custody.
    /// </summary>
    public class KkhayClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly bool _disposeClient;
        private readonly string _baseUrl;

        public KkhayClient(string apiKey, string baseUrl = "https://api.kkhay.com", TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("apiKey is required and cannot be empty.", nameof(apiKey));

            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey.Trim());
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "kkhay-dotnet/1.0.4");
            _disposeClient = true;
        }

        public KkhayClient(HttpClient httpClient, string apiKey, string baseUrl = "https://api.kkhay.com")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey.Trim());
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "kkhay-dotnet/1.0.4");
            _disposeClient = false;
        }

        private string GetUrl(string path)
        {
            var cleanPath = path.StartsWith("/") ? path : $"/{path}";
            if (_baseUrl.EndsWith("/api") || _baseUrl.Contains("api."))
                return $"{_baseUrl}{cleanPath}";
            return $"{_baseUrl}/api{cleanPath}";
        }

        private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new KkhayApiException(408, "Request timed out after configured duration", null, ex);
            }
            catch (Exception ex)
            {
                throw new KkhayApiException(500, $"Network error: {ex.Message}", null, ex);
            }

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                string message = $"Request failed with status {(int)response.StatusCode}";
                string? code = null;
                object? details = null;

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("message", out var msgElem))
                        message = msgElem.GetString() ?? message;
                    else if (doc.RootElement.TryGetProperty("error", out var errElem))
                        message = errElem.GetString() ?? message;

                    if (doc.RootElement.TryGetProperty("code", out var codeElem))
                        code = codeElem.GetString();

                    if (doc.RootElement.TryGetProperty("details", out var detElem))
                        details = detElem.ToString();
                }
                catch { }

                throw new KkhayApiException((int)response.StatusCode, message, code, details);
            }

            var result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result ?? throw new InvalidOperationException("Empty response received from gateway.");
        }

        /// <summary>
        /// Create a new non-custodial crypto payment invoice.
        /// </summary>
        public async Task<CreateInvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
        {
            if (request.ResolvedAmount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(request));

            // Ensure symmetric population of price/amount aliases
            request.Amount ??= request.PriceAmount;
            request.PriceAmount ??= request.Amount;
            request.Currency ??= request.PriceCurrency ?? "USD";
            request.PriceCurrency ??= request.Currency ?? "USD";

            var url = GetUrl("/v1/gateway/invoices");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            var payload = JsonSerializer.Serialize(request);
            httpRequest.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            return await SendAsync<CreateInvoiceResponse>(httpRequest, cancellationToken);
        }

        /// <summary>
        /// Retrieve the status, blockchain transactions, and details of a specific invoice.
        /// </summary>
        public async Task<GetInvoiceResponse> GetInvoiceAsync(string invoiceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentException("invoiceId is required", nameof(invoiceId));

            var url = GetUrl($"/v1/gateway/invoices/{Uri.EscapeDataString(invoiceId.Trim())}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);

            return await SendAsync<GetInvoiceResponse>(httpRequest, cancellationToken);
        }

        /// <summary>
        /// List and filter invoices generated by your gateway account.
        /// </summary>
        public async Task<ListInvoicesResponse> ListInvoicesAsync(
            int page = 1,
            int limit = 20,
            string? status = null,
            string? type = null,
            string? search = null,
            CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"limit={limit}" };
            if (!string.IsNullOrWhiteSpace(status)) query.Add($"status={Uri.EscapeDataString(status.Trim().ToUpperInvariant())}");
            if (!string.IsNullOrWhiteSpace(type)) query.Add($"type={Uri.EscapeDataString(type.Trim().ToUpperInvariant())}");
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search.Trim())}");

            var url = GetUrl($"/v1/gateway/invoices?{string.Join("&", query)}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);

            return await SendAsync<ListInvoicesResponse>(httpRequest, cancellationToken);
        }

        public void Dispose()
        {
            if (_disposeClient)
                _httpClient.Dispose();
        }
    }
}
