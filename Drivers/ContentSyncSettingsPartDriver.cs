using System.Collections.Generic;
using System.Linq;
using CJP.ContentSync.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ImportExport.Services;
using Orchard.Localization;

namespace CJP.ContentSync.Drivers {

    // We define a specific driver instead of using a TemplateFilterForRecord, because we need the model to be the part and not the record.

    public class ContentSyncSettingsPartDriver : ContentPartDriver<ContentSyncSettingsPart>
    {
        private readonly IContentManager _contentManager;
        private readonly ICustomExportStep _customExportSteps;
        private readonly IOrchardServices _orchardServices;
        private const string TemplateName = "Parts/ContentSyncSettingsPart";

        public ContentSyncSettingsPartDriver(IContentManager contentManager, ICustomExportStep customExportSteps, IOrchardServices orchardServices) {
            _contentManager = contentManager;
            _customExportSteps = customExportSteps;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "ContentSyncSettings"; } }

        protected override DriverResult Editor(ContentSyncSettingsPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_ContentSyncSettings_Edit",
                () => {
                    var customSteps = new List<string>();
                    _customExportSteps.Register(customSteps);

                    part.AllContentTypes = _contentManager.GetContentTypeDefinitions().OrderBy(ctd => ctd.Name).Select(ctd => new SelectableItem<string> { Item = ctd.Name, IsSelected = part.ExcludedContentTypes.Contains(ctd.Name) }).ToList();
                    part.AllExportSteps = customSteps.OrderBy(n => n).Select(cs => new SelectableItem<string> { Item = cs, IsSelected = part.ExcludedExportSteps.Contains(cs) }).ToList();
                    part.AllSiteSettings = GetExportableSettings().OrderBy(n => n).Select(p => new SelectableItem<string> { Item = p, IsSelected = part.ExcludedSiteSettings.Contains(p) }).ToList();
                    
                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
                })
                .OnGroup("Content Sync");
        }

        protected override DriverResult Editor(ContentSyncSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_ContentSyncSettings_Edit", () =>
            {
                updater.TryUpdateModel(part, Prefix, null, null);
                part.ExcludedContentTypes = part.AllContentTypes.Where(i => i.IsSelected).Select(i => i.Item).ToArray();
                part.ExcludedExportSteps = part.AllExportSteps.Where(i => i.IsSelected).Select(i => i.Item).ToArray();
                part.ExcludedSiteSettings = part.AllSiteSettings.Where(i => i.IsSelected).Select(i => i.Item).ToArray();

                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            }).OnGroup("Content Sync");
        }

        private IEnumerable<string> GetExportableSettings()
        {
            var settings = new List<string>();

            foreach (var sitePart in _orchardServices.WorkContext.CurrentSite.ContentItem.Parts)
            {
                foreach (var property in sitePart.GetType().GetProperties())
                {
                    var propertyType = property.PropertyType;
                    // Supported types (we also know they are not indexed properties).
                    if (propertyType == typeof(string) || propertyType == typeof(bool) || propertyType == typeof(int))
                    {
                        // Exclude read-only properties.
                        if (property.GetSetMethod() != null)
                        {
                            settings.Add(sitePart.PartDefinition.Name);
                            break;
                        }
                    }
                }
            }

            return settings;
        }
    }
}