namespace Aghili.Logging.Classes;

public interface IWriterOption
{
    int LogKeepDataOnLimitRichedPercent { get; set; }

    int LogRowLimitPerContainer { get; set; }
}