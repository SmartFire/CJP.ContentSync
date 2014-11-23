using CJP.ContentSync.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;

namespace CJP.ContentSync.Handlers {
    public class ContentSyncSettingsPartHandler : ContentHandler {
        public ContentSyncSettingsPartHandler(IRepository<ContentSyncSettingsRecord> repository)
        {
            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<ContentSyncSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);

            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Content Sync")));
        }
    }
}