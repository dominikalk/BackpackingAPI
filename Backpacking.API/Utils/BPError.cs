using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;

namespace Backpacking.API.Utils;

public sealed class BPError
{
    public static BPError Default => new BPError();

    public HttpStatusCode Code { get; init; }
    public string Message { get; init; } = string.Empty;

    public BPError() { }

    public BPError(HttpStatusCode code, string message) : this()
    {
        Code = code;
        Message = message;
    }

    public override bool Equals(object? value)
    {
        return value is BPError error &&
            Code == error.Code &&
            Message == error.Message;
    }

    public override int GetHashCode()
        => HashCode.Combine(Code, Message);
}
