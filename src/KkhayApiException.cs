using System;

namespace Kkhay
{
    /// <summary>
    /// Exception thrown when a K Khay API endpoint returns an error.
    /// </summary>
    public class KkhayApiException : Exception
    {
        public int StatusCode { get; }
        public string? ErrorCode { get; }
        public object? Details { get; }

        public KkhayApiException(int statusCode, string message, string? errorCode = null, object? details = null)
            : base($"KkhayApiException [HTTP {statusCode}]: {message}")
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Details = details;
        }
    }

    /// <summary>
    /// Exception thrown when webhook HMAC verification fails.
    /// </summary>
    public class SignatureVerificationException : Exception
    {
        public SignatureVerificationException(string message) : base(message)
        {
        }
    }
}

