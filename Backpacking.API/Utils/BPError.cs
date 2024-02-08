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
}
