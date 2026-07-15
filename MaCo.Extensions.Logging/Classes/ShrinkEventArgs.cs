namespace MaCo.Extensions.Logging.Classes;

public class ShrinkEventArgs : EventArgs
{
    public ShrinkType Type { set; get; } = ShrinkType.Backup;

    public int RecordCount { set; get; }

    public int NewRecordCount { set; get; }
}