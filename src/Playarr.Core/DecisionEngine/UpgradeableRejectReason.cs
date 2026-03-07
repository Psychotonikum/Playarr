namespace Playarr.Core.DecisionEngine
{
    public enum UpgradeableRejectReason
    {
        None,
        BetterQuality,
        BetterRevision,
        QualityCutoff,
        CustomFormatScore,
        CustomFormatCutoff,
        MinCustomFormatScore,
        UpgradesNotAllowed
    }
}
