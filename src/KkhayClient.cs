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
    public class KkhayClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly bool _disposeClient;
        private readonly string _baseUrl;

        public KkhayClient(string apiKey, string baseUrl = "https://api.kkhay.com", TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("apiKey is required", nameof(apiKey));

            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey.Trim());
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "kkhay-dotnet/1.0.0");
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
                throw new KkhayApiException(408, "Request timed out", null, ex);
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
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("message", out var msgElem))
                        message = msgElem.GetString() ?? message;
                    else if (doc.RootElement.TryGetProperty("error", out var errElem))
                        message = errElem.GetString() ?? message;

                    if (doc.RootElement.TryGetProperty("code", out var codeElem))
                        code = codeElem.GetString();
                }
                catch { }

                throw new KkhayApiException((int)response.StatusCode, message, code);
            }

            var result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result ?? throw new InvalidOperationException("Empty response received from gateway.");
        }

        public async Task<CreateInvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
        {
            if (request.PriceAmount <= 0)
                throw new ArgumentException("PriceAmount must be greater than 0", nameof(request));
            if (string.IsNullOrWhiteSpace(request.PayNetwork))
                throw new ArgumentException("PayNetwork is required", nameof(request));
            if (string.IsNullOrWhiteSpace(request.PayToken))
                throw new ArgumentException("PayToken is required", nameof(request));

            var url = GetUrl("/v1/merchant/invoices");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            var payload = JsonSerializer.Serialize(request);
            httpRequest.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            return await SendAsync<CreateInvoiceResponse>(httpRequest, cancellationToken);
        }

        public async Task<GetInvoiceResponse> GetInvoiceAsync(string invoiceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentException("invoiceId is required", nameof(invoiceId));

            var url = GetUrl($"/v1/merchant/invoices/{Uri.EscapeDataString(invoiceId.Trim())}");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);

            return await SendAsync<GetInvoiceResponse>(httpRequest, cancellationToken);
        }

        public async Task<ListInvoicesResponse> ListInvoicesAsync(int page = 1, int limit = 20, string? status = null, string? search = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"page={page}", $"limit={limit}" };
            if (!string.IsNullOrWhiteSpace(status)) query.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");

            var url = GetUrl($"/v1/merchant/invoices?{string.Join("&", query)}");
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

