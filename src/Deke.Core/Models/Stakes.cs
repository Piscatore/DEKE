namespace Deke.Core.Models;

/// <summary>
/// Caller-supplied stakes hint. Higher stakes can trigger model escalation.
/// </summary>
public enum Stakes
{
    Low = 0,
    Medium = 1,
    High = 2
}
