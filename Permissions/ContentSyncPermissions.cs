using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace CJP.ContentSync.Permissions
{
    public class ContentSyncPermissions : IPermissionProvider
    {
        public static readonly Permission ContentSyncUI = new Permission { Description = "Provides access to the content sync UI", Name = "ContentSyncUI" };
        public static readonly Permission RedactionManager = new Permission { Description = "Allows this user to manage the redaction setting for content syncing", Name = "RedactionManager" };
        public static readonly Permission SnapshotManager = new Permission { Description = "Allows this user to manage and take content and config Snapshot", Name = "SnapshotManager" };
        public static readonly Permission ContentExportApi = new Permission { Description = "Allows a user's credentials to be used to autheticate a Content Export API request", Name = "ContentExportApi" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                ContentExportApi,
                ContentSyncUI,
                SnapshotManager,
                RedactionManager
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ContentSyncUI, RedactionManager, SnapshotManager}
                }
            };
        }
    }
}


