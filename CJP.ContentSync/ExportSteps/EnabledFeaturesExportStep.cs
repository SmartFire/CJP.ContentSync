﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard;
using Orchard.Environment.Features;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Logging;

namespace CJP.ContentSync.ExportSteps
{
    public class EnabledFeaturesExportStep : IExportEventHandler, ICustomExportStep
    {
        private readonly IFeatureManager _featureManager;

        public EnabledFeaturesExportStep(IFeatureManager featureManager) 
        {
            _featureManager = featureManager;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void Exporting(ExportContext context) {
            if (!context.ExportOptions.CustomSteps.Contains("Features")) { return; }

            var xmlElement = new XElement("EnabledFeatures");

            foreach (var feature in _featureManager.GetEnabledFeatures()) {
                xmlElement.Add(new XElement("Feature", new XAttribute("Id", feature.Id)));
            }

            var rootElement = context.Document.Descendants("Orchard").FirstOrDefault();

            if (rootElement == null) 
            {
                var ex = new OrchardException(T("Could not export this site's Enabled Features because the document passed via the Export Context did not contain a node called 'Orchard'. The document was malformed."));
                Logger.Error(ex, ex.Message);
                throw ex;
            }

            rootElement.AddFirst(xmlElement);
        }

        public void Exported(ExportContext context) {}

        public void Register(IList<string> steps) {
            steps.Add("Features");
        }
    }
}