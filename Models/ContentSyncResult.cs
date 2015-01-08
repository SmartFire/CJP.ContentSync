using CJP.ContentSync.Models.Enums;

namespace CJP.ContentSync.Services {
    public class ContentSyncResult
    {
        public ContentSyncResultStatus Status { get; set; }
        public string RecipeExecutionId { get; set; }
    }
}