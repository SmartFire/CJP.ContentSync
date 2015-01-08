using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public string GetContentExportFilePath() {
            var settings = _orchardServices.WorkContext.CurrentSite.As<ContentSyncSettingsPart>();

            var contentTypes = _contentManager.GetContentTypeDefinitions().Select(ctd => ctd.Name).Except(settings.ExcludedContentTypes).ToList();

            var customSteps = new List<string>();
            _customExportStep.Register(customSteps);
            customSteps = customSteps.Except(settings.ExcludedExportSteps).ToList();

            return _importExportService.Export(contentTypes, new ExportOptions { CustomSteps = customSteps, ExportData = true, ExportMetadata = true, ExportSiteSettings = false, VersionHistoryOptions = VersionHistoryOptions.Published });
        }

        public string GetContentExportText() {
            var filePath = GetContentExportFilePath();

            return File.ReadAllText(filePath);
        }
    }
}