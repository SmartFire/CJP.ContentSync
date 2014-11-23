using Orchard.ContentManagement.Records;

namespace CJP.ContentSync.Models
{
    public class ContentSyncSettingsRecord : ContentPartRecord
    {
        public virtual string ExcludedExportSteps { get; set; }
        public virtual string ExcludedSiteSettings { get; set; }
        public virtual string ExcludedContentTypes { get; set; }
    }
}