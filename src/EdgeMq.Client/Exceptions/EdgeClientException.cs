using System;
using System.Net;

namespace EdgeMq.Client.Exceptions;

public sealed class EdgeClientException : Exception
{
    public EdgeClientException(string message) : base(message) { }

    public EdgeClientException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; private init; } = HttpStatusCode.Accepted;
}