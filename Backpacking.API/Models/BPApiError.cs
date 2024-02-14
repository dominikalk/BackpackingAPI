using Backpacking.API.Utils;

namespace Backpacking.API.Models;

public class BPApiError
{
    public BPError Error { get; set; } = BPError.Default;

    public BPApiError(BPError error)
    {
        Error = error;
    }
}
