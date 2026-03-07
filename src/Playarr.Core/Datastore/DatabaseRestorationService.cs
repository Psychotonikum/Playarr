using System;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation;

namespace Playarr.Core.Datastore
{
    public interface IRestoreDatabase
    {
        void Restore();
    }

    public class DatabaseRestorationService : IRestoreDatabase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAppFolderInfo _appFolderInfo;
        private static readonly Logger Logger = PlayarrLogger.GetLogger(typeof(DatabaseRestorationService));

        public DatabaseRestorationService(IDiskProvider diskProvider, IAppFolderInfo appFolderInfo)
        {
            _diskProvider = diskProvider;
            _appFolderInfo = appFolderInfo;
        }

        public void Restore()
        {
            var dbRestorePath = _appFolderInfo.GetDatabaseRestore();

            if (!_diskProvider.FileExists(dbRestorePath))
            {
                return;
            }

            try
            {
                Logger.Info("Restoring Database");

                var dbPath = _appFolderInfo.GetDatabase();

                _diskProvider.DeleteFile(dbPath + "-shm");
                _diskProvider.DeleteFile(dbPath + "-wal");
                _diskProvider.DeleteFile(dbPath + "-journal");
                _diskProvider.DeleteFile(dbPath);

                _diskProvider.MoveFile(dbRestorePath, dbPath);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to restore database");
                throw;
            }
        }
    }
}
