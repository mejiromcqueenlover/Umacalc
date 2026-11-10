namespace Umacalc.Models;

public class TurnRecord
{
    public int TurnIndex { get; set; }
    public StatType Stat { get; set; }
    public string StatName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#3B82F6";
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Note { get; set; } = string.Empty;

    public string FormattedDescription => $"Trainable Turn #{TurnIndex}: {StatName} ({Note})";
}
