using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CJP.ContentSync.Models.ViewModels {
    public class AdminImportVM
    {
        public AdminImportVM() {
            SavedRemoteSiteConfigs = new List<RemoteSiteConfigRecord>();
        }

        [Required]
        public string Url { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public List<RemoteSiteConfigRecord> SavedRemoteSiteConfigs { get; set; }
    }
}