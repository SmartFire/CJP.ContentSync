using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CJP.ContentSync.Models;
using CJP.ContentSync.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Logging;

namespace CJP.ContentSync.ExportSteps
{
    public class RedactedSiteSettingsExportStep : IExportEventHandler, ICustomExportStep
    {
        private readonly ITextRedactionService _textRedactionService;
        private readonly IOrchardServices _orchardServices;

        public RedactedSiteSettingsExportStep(ITextRedactionService textRedactionService, IOrchardServices orchardServices) 
        {
            _textRedactionService = textRedactionService;
            _orchardServices = orchardServices;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void Exporting(ExportContext context) {
            if (!context.ExportOptions.CustomSteps.Contains("RedactedSiteSettings")) { return; }

            var excludedSettings = _orchardServices.WorkContext.CurrentSite.As<ContentSyncSettingsPart>().ExcludedSiteSettings;

            var settings = new XElement("RedactedSiteSettings");
            var hasSetting = false;

            foreach (var sitePart in _orchardServices.WorkContext.CurrentSite.ContentItem.Parts) {
                if (excludedSettings.Contains(sitePart.PartDefinition.Name)) {
                    continue;
                }

                var setting = new XElement(sitePart.PartDefinition.Name);

                foreach (var property in sitePart.GetType().GetProperties()) {
                    var propertyType = property.PropertyType;
                    // Supported types (we also know they are not indexed properties).
                    if (propertyType == typeof(string) || propertyType == typeof(bool) || propertyType == typeof(int)) {
                        // Exclude read-only properties.
                        if (property.GetSetMethod() != null) {
                            setting.SetAttributeValue(property.Name, _textRedactionService.RedactText(Convert.ToString(property.GetValue(sitePart, null))));
                            hasSetting = true;
                        }
                    }
                }

                if (hasSetting) {
                    settings.Add(setting);
                    hasSetting = false;
                }
            }

            var rootElement = context.Document.Descendants("Orchard").FirstOrDefault();

            if (rootElement == null) 
            {
                var ex = new OrchardException(T("Could not export this site's Redacted Settings because the document passed via the Export Context did not contain a node called 'Orchard'. The document was malformed."));
                Logger.Error(ex, ex.Message);
                throw ex;
            }

            rootElement.Add(settings);
        }

        public void Exported(ExportContext context) {}

        public void Register(IList<string> steps) {
            steps.Add("RedactedSiteSettings");
        }
    }
}