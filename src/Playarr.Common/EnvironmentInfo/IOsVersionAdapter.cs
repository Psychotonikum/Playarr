namespace Playarr.Common.EnvironmentInfo
{
    public interface IOsVersionAdapter
    {
        bool Enabled { get; }
        OsVersionModel Read();
    }
}
