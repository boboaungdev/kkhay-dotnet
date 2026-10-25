using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kkhay.Models
{
    /// <summary>
    /// Real-time invoice status in K Khay Gateway.
    /// Matches PostgreSQL pgEnum "GatewayInvoiceStatus".
    /// </summary>
    public static class GatewayInvoiceStatus
    {
        public const string Waiting = "WAITING";
        public const string Paid = "PAID";
        public const string Expired = "EXPIRED";
        public const string Canceled = "CANCELED";
    }

    /// <summary>
    /// Classification of invoice intent.
    /// Matches PostgreSQL pgEnum "GatewayInvoiceType".
    /// </summary>
    public static class GatewayInvoiceType
    {
        public const string Payment = "PAYMENT";
        public const string Donation = "DONATION";
        public const string Subscription = "SUBSCRIPTION";
    }

    /// <summary>
    /// On-chain payment settlement status.
    /// Matches PostgreSQL pgEnum "GatewayPaymentStatus".
    /// </summary>
    public static class GatewayPaymentStatus
    {
        public const string Settling = "SETTLING";
        public const string Settled = "SETTLED";
        public const string Failed = "FAILED";
    }

    /// <summary>
    /// Parameters required to create a new payment invoice on K Khay Gateway.
    /// </summary>
    public class CreateInvoiceRequest
    {
        [JsonPropertyName("amount")]
        public decimal? Amount { get; set; }

        [JsonPropertyName("priceAmount")]
        public decimal? PriceAmount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("priceCurrency")]
        public string? PriceCurrency { get; set; } = "USD";

        [JsonPropertyName("payNetwork")]
        public string? PayNetwork { get; set; }

        [JsonPropertyName("payToken")]
        public string? PayToken { get; set; }

        [JsonPropertyName("settlementAsset")]
        public string? SettlementAsset { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; } = GatewayInvoiceType.Payment;

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

        /// <summary>
        /// Gets the resolved charge amount.
        /// </summary>
        [JsonIgnore]
        public decimal ResolvedAmount => Amount ?? PriceAmount ?? 0m;

        /// <summary>
        /// Gets the resolved charge currency code.
        /// </summary>
        [JsonIgnore]
        public string ResolvedCurrency => Currency ?? PriceCurrency ?? "USD";
    }

    /// <summary>
    /// Fully typed invoice model matching K Khay Gateway PostgreSQL schema.
    /// </summary>
    public class Invoice
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("gatewayId")]
        public string GatewayId { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = GatewayInvoiceType.Payment;

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";

        [JsonPropertyName("priceAmount")]
        public decimal PriceAmount { get; set; }

        [JsonPropertyName("priceCurrency")]
        public string PriceCurrency { get; set; } = "USD";

        [JsonPropertyName("settlementAsset")]
        public string? SettlementAsset { get; set; }

        [JsonPropertyName("payAmount")]
        public string? PayAmount { get; set; }

        [JsonPropertyName("payToken")]
        public string? PayToken { get; set; }

        [JsonPropertyName("payNetwork")]
        public string? PayNetwork { get; set; }

        [JsonPropertyName("lockedRate")]
        public string? LockedRate { get; set; }

        [JsonPropertyName("depositAddress")]
        public string DepositAddress { get; set; } = string.Empty;

        [JsonPropertyName("derivationIndex")]
        public int? DerivationIndex { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = GatewayInvoiceStatus.Waiting;

        [JsonPropertyName("feeAmount")]
        public string FeeAmount { get; set; } = "0";

        [JsonPropertyName("netAmount")]
        public string NetAmount { get; set; } = "0";

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

        [JsonPropertyName("paymentMethod")]
        public string? PaymentMethod { get; set; }

        [JsonPropertyName("settlementMode")]
        public string? SettlementMode { get; set; }

        [JsonPropertyName("payoutAddress")]
        public string PayoutAddress { get; set; } = string.Empty;

        [JsonPropertyName("hostedUrl")]
        public string HostedUrl { get; set; } = string.Empty;

        [JsonPropertyName("expiresAt")]
        public string ExpiresAt { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("updatedAt")]
        public string? UpdatedAt { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// On-chain payment transaction confirmed on the blockchain.
    /// </summary>
    public class PaymentRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("invoiceId")]
        public string InvoiceId { get; set; } = string.Empty;

        [JsonPropertyName("txHash")]
        public string TxHash { get; set; } = string.Empty;

        [JsonPropertyName("fromAddress")]
        public string? FromAddress { get; set; }

        [JsonPropertyName("amountReceived")]
        public string AmountReceived { get; set; } = string.Empty;

        [JsonPropertyName("confirmations")]
        public int Confirmations { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = GatewayPaymentStatus.Settling;

        [JsonPropertyName("forwardedTxHash")]
        public string? ForwardedTxHash { get; set; }

        [JsonPropertyName("forwardedAt")]
        public string? ForwardedAt { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response returned upon creating an invoice.
    /// </summary>
    public class CreateInvoiceResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("invoice")]
        public Invoice Invoice { get; set; } = new();
    }

    /// <summary>
    /// Response returned when looking up an invoice by ID.
    /// </summary>
    public class GetInvoiceResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("invoice")]
        public Invoice Invoice { get; set; } = new();

        [JsonPropertyName("payments")]
        public List<PaymentRecord> Payments { get; set; } = new();
    }

    /// <summary>
    /// Response returned when listing invoices with pagination.
    /// </summary>
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

    /// <summary>
    /// Strongly-typed Webhook Event dispatched by K Khay IPN engine.
    /// </summary>
    public class WebhookEvent
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("invoice_id")]
        public string InvoiceId { get; set; } = string.Empty;

        [JsonPropertyName("invoice_type")]
        public string? InvoiceType { get; set; }

        [JsonPropertyName("gateway_id")]
        public string GatewayId { get; set; } = string.Empty;

        [JsonPropertyName("order_id")]
        public string? OrderId { get; set; }

        [JsonPropertyName("amount")]
        public decimal? Amount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("price_amount")]
        public decimal PriceAmount { get; set; }

        [JsonPropertyName("price_currency")]
        public string PriceCurrency { get; set; } = "USD";

        [JsonPropertyName("pay_amount")]
        public string? PayAmount { get; set; }

        [JsonPropertyName("pay_token")]
        public string? PayToken { get; set; }

        [JsonPropertyName("pay_network")]
        public string? PayNetwork { get; set; }

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
