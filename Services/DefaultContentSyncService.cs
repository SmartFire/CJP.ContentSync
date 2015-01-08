using System;
using System.Net;
using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using Orchard.Data;
using Orchard.ImportExport.Services;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Services;

namespace CJP.ContentSync.Services {
    public class DefaultContentSyncService : IContentSyncService 
    {
        private readonly IRepository<RemoteSiteConfigRecord> _remoteConfigRepository;
        private readonly IClock _clock;
        private readonly IImportExportService _importExportService;
        private readonly IRecipeJournal _recipeJournal;

        public DefaultContentSyncService(IRepository<RemoteSiteConfigRecord> remoteConfigRepository, IClock clock, IImportExportService importExportService, IRecipeJournal recipeJournal)
        {
            _remoteConfigRepository = remoteConfigRepository;
            _clock = clock;
            _importExportService = importExportService;
            _recipeJournal = recipeJournal;

            Logger = NullLogger.Instance;
        }
        public ILogger Logger { get; set; }

        public ContentSyncResult Sync(int remoteSiteConfigId)
        {
            var config = _remoteConfigRepository.Get(remoteSiteConfigId);

            if (config == null) {
                return new ContentSyncResult { Status = ContentSyncResultStatus.RemoteSiteConfigDoesNotExist };
            }

            var result = Sync(config.Url, config.Username, config.Password);

            if (result.Status == ContentSyncResultStatus.OK) {
                config.LastSynced = _clock.UtcNow;
            }

            return result;
        }

        public ContentSyncResult Sync(string url, string username, string password) 
        {
            url = string.Format("{0}/contentsync/contentExport?username={1}&password={2}", url, username, password);

            return Sync(url);
        }

        public ContentSyncResult Sync(string url) {
            var importText = string.Empty;

            try
            {
                var client = new ExtendedTimeoutWebClient();

                importText = client.DownloadString(url);
            }
            catch (WebException ex)
            {
                var httpWebResponse = ((HttpWebResponse)ex.Response);

                if (httpWebResponse == null)
                {
                    Logger.Error(ex, "There was an error exporting the remote site at {0}. The error response did not contain an HTTP status code; this implies that there was a connectivity issue, or the request timed out", url);

                    return new ContentSyncResult { Status = ContentSyncResultStatus.RemoteUrlTimedout };
                }

                var statusCode = httpWebResponse.StatusCode;

                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    return new ContentSyncResult { Status = ContentSyncResultStatus.RemoteUrlUnauthorized };
                }

                if (statusCode != HttpStatusCode.OK)
                {
                    Logger.Error(ex, "There was an error exporting the remote site at {0}", url);

                    return new ContentSyncResult { Status = ContentSyncResultStatus.RemoteUrlFailed };
                }
            }

            return SyncFromText(importText);
        }
        public ContentSyncResult SyncFromText(string text)
        {
            var executionId = _importExportService.Import(text);
            var journal = _recipeJournal.GetRecipeJournal(executionId);

            var status = ContentSyncResultStatus.RecipeExecutionPending;

            switch (journal.Status)
            {
                case RecipeStatus.Complete:
                    status = ContentSyncResultStatus.OK;
                    break;
                case RecipeStatus.Failed:
                    status = ContentSyncResultStatus.RecipeExecutionFailed;
                    break;
            }

            return new ContentSyncResult { Status = status, RecipeExecutionId = executionId};
        }

        class ExtendedTimeoutWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 3 * 60 * 1000; //3 minutes

                return w;
            }
        }
    }
}