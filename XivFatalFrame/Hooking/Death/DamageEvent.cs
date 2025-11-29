namespace XivFatalFrame.Hooking.Death;

internal readonly struct DamageEvent
{
    public readonly string? SourceName;
    public readonly uint?   Action;
    public readonly uint?   Amount;

    public DamageEvent(uint? amount)
    {
        SourceName = null;
        Action     = null;
        Amount     = amount;
    }

    public DamageEvent(string? sourceName, uint? action, uint? amount)
    {
        SourceName  = sourceName;
        Action      = action;
        Amount      = amount;
    }

    public override string ToString() => $"{SourceName} {Action} {Amount}";
}
