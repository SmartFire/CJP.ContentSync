using System.Collections.Generic;
using Orchard.ContentManagement;

namespace CJP.ContentSync.Models 
{
    public class ContentSyncSettingsPart : ContentPart<ContentSyncSettingsRecord>
    {
        public string[] ExcludedExportSteps
        {
            get { return (Record.ExcludedExportSteps ?? "").Split('#'); }
            set { Record.ExcludedExportSteps = string.Join("#", value); }
        }

        public string[] ExcludedSiteSettings
        {
            get { return (Record.ExcludedSiteSettings ?? "").Split('#'); }
            set { Record.ExcludedSiteSettings = string.Join("#", value); }
        }

        public string[] ExcludedContentTypes
        {
            get { return (Record.ExcludedContentTypes ?? "").Split('#'); }
            set { Record.ExcludedContentTypes = string.Join("#", value); }
        }

        public IList<SelectableItem<string>> AllContentTypes { get; set; }
        public IList<SelectableItem<string>> AllExportSteps { get; set; }
        public IList<SelectableItem<string>> AllSiteSettings { get; set; }
    }
}