using Orchard;

namespace CJP.ContentSync.Services {
    public interface IContentSyncService : IDependency
    {
        ContentSyncResult Sync(int remoteSiteConfigId);
        ContentSyncResult Sync(string url, string username, string password);
        ContentSyncResult Sync(string url);
        ContentSyncResult SyncFromText(string text);
    }
}