namespace CJP.ContentSync.Models.Enums {
    public enum ContentSyncResultStatus {
        RemoteSiteConfigDoesNotExist,
        RemoteUrlTimedout,
        RemoteUrlFailed,
        RemoteUrlUnauthorized,
        RecipeExecutionFailed,
        RecipeExecutionPending,
        OK
    }
}