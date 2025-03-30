namespace Aghili.Logging.Classes;

public class ShirinkEventArgs : EventArgs
{
    public ShirinkType Type { set; get; } = ShirinkType.Backup;

    public int RecordCount { set; get; }

    public int NewRecordCount { set; get; }
}