using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CJP.ContentSync.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Logging;

namespace CJP.ContentSync.Services {
    public class DefaultContentExportService : IContentExportService {
        private readonly IImportExportService _importExportService;
        private readonly IContentManager _contentManager;
        private readonly ICustomExportStep _customExportStep;
        private readonly IOrchardServices _orchardServices;

        public DefaultContentExportService(IImportExportService importExportService, IContentManager contentManager, ICustomExportStep customExportStep, IOrchardServices orchardServices)
        {
            _importExportService = importExportService;
            _contentManager = contentManager;
            _customExportStep = customExportStep;
            _orchardServices = orchardServices;

            Logger = NullLogger.Instance;
        }
        public ILogger Logger { get; set; }

        public string GetContentExportText() {
            var settings = _orchardServices.WorkContext.CurrentSite.As<ContentSyncSettingsPart>();

            var contentTypes = _contentManager.GetContentTypeDefinitions().Select(ctd => ctd.Name).Except(settings.ExcludedContentTypes).ToList();

            var customSteps = new List<string>();
            _customExportStep.Register(customSteps);
            customSteps = customSteps.Except(settings.ExcludedExportSteps).ToList();

            return _importExportService.Export(contentTypes, new ExportOptions { CustomSteps = customSteps, ExportData = true, ExportMetadata = true, ExportSiteSettings = false, VersionHistoryOptions = VersionHistoryOptions.Published });
        }

        public ApiResult GetContentExportFromUrl(string url, string username, string password)
        {
            url = string.Format("{0}/contentsync/contentExport?username={1}&password={2}", url, username, password);
            var importText = string.Empty;

            try {
                var client = new ExtendedTimeoutWebClient();

                importText = client.DownloadString(url);
            }
            catch (WebException ex) 
            {
                var httpWebResponse = ((HttpWebResponse) ex.Response);

                if (httpWebResponse == null)
                {
                    Logger.Log(LogLevel.Error, ex, "There was an error exporting the remote site at {0}. The error response did not contain an HTTP status code; this implies that there was a connectivity issue, or the request timed out", url);

                    return new ApiResult { Status = ApiResultStatus.Failed };
                }

                var statusCode = httpWebResponse.StatusCode;

                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    return new ApiResult { Status = ApiResultStatus.Unauthorized };
                }

                if (statusCode != HttpStatusCode.OK)
                {
                    Logger.Log(LogLevel.Error, ex, "There was an error exporting the remote site at {0}", url);

                    return new ApiResult { Status = ApiResultStatus.Failed };
                }
            }

            return new ApiResult { Status = ApiResultStatus.OK, Text = importText };
        }
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