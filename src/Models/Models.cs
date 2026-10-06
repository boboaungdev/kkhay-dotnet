using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kkhay.Models
{
    public class CreateInvoiceRequest
    {
        [JsonPropertyName("priceAmount")]
        public decimal PriceAmount { get; set; }

        [JsonPropertyName("priceCurrency")]
        public string? PriceCurrency { get; set; } = "USD";

        [JsonPropertyName("payNetwork")]
        public string PayNetwork { get; set; } = string.Empty;

        [JsonPropertyName("payToken")]
        public string PayToken { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("customerName")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("customerEmail")]
        public string? CustomerEmail { get; set; }

        [JsonPropertyName("redirectUrl")]
        public string? RedirectUrl { get; set; }

        [JsonPropertyName("cancelUrl")]
        public string? CancelUrl { get; set; }

        [JsonPropertyName("ipnCallbackUrl")]
        public string? IpnCallbackUrl { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class Invoice
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("merchantId")]
        public string MerchantId { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("priceAmount")]
        public decimal PriceAmount { get; set; }

        [JsonPropertyName("priceCurrency")]
        public string PriceCurrency { get; set; } = "USD";

        [JsonPropertyName("payAmount")]
        public string PayAmount { get; set; } = string.Empty;

        [JsonPropertyName("payToken")]
        public string PayToken { get; set; } = string.Empty;

        [JsonPropertyName("payNetwork")]
        public string PayNetwork { get; set; } = string.Empty;

        [JsonPropertyName("depositAddress")]
        public string DepositAddress { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = "WAITING";

        [JsonPropertyName("feeAmount")]
        public string FeeAmount { get; set; } = "0";

        [JsonPropertyName("netAmount")]
        public string NetAmount { get; set; } = "0";

        [JsonPropertyName("hostedUrl")]
        public string HostedUrl { get; set; } = string.Empty;

        [JsonPropertyName("redirectUrl")]
        public string? RedirectUrl { get; set; }

        [JsonPropertyName("cancelUrl")]
        public string? CancelUrl { get; set; }

        [JsonPropertyName("expiresAt")]
        public string ExpiresAt { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class PaymentRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("txHash")]
        public string TxHash { get; set; } = string.Empty;

        [JsonPropertyName("amountReceived")]
        public string AmountReceived { get; set; } = string.Empty;

        [JsonPropertyName("confirmations")]
        public int Confirmations { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("forwardedTxHash")]
        public string? ForwardedTxHash { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class CreateInvoiceResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("invoice")]
        public Invoice Invoice { get; set; } = new();
    }

    public class GetInvoiceResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("invoice")]
        public Invoice Invoice { get; set; } = new();

        [JsonPropertyName("payments")]
        public List<PaymentRecord> Payments { get; set; } = new();
    }

    public class ListInvoicesResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("items")]
        public List<Invoice> Items { get; set; } = new();

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }
    }

    public class WebhookEvent
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("invoice_id")]
        public string InvoiceId { get; set; } = string.Empty;

        [JsonPropertyName("order_id")]
        public string? OrderId { get; set; }

        [JsonPropertyName("price_amount")]
        public decimal PriceAmount { get; set; }

        [JsonPropertyName("price_currency")]
        public string PriceCurrency { get; set; } = "USD";

        [JsonPropertyName("pay_amount")]
        public string PayAmount { get; set; } = string.Empty;

        [JsonPropertyName("pay_token")]
        public string PayToken { get; set; } = string.Empty;

        [JsonPropertyName("pay_network")]
        public string PayNetwork { get; set; } = string.Empty;

        [JsonPropertyName("deposit_address")]
        public string DepositAddress { get; set; } = string.Empty;

        [JsonPropertyName("tx_hash")]
        public string? TxHash { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;
    }
}

