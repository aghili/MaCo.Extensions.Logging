namespace Aghili.Logging.Classes;

public class WriteOptionEvent : IWriterOption
{
    public int LogKeepDataOnLimitRichedPercent { get; set; } = 80;

    public int LogRowLimitPerContainer { get; set; } = 20;
}