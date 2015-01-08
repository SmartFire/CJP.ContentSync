using System.Linq;
using System.Web.Mvc;
using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using CJP.ContentSync.Models.ViewModels;
using CJP.ContentSync.Services;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.UI.Notify;

namespace CJP.ContentSync.Controllers
{
    public class AdminController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<RemoteSiteConfigRecord> _remoteConfigRepository;
        private readonly IContentSyncService _contentSyncService;

        public AdminController(IOrchardServices orchardServices, IRepository<RemoteSiteConfigRecord> remoteConfigRepository, IContentSyncService contentSyncService) {
            _orchardServices = orchardServices;
            _remoteConfigRepository = remoteConfigRepository;
            _contentSyncService = contentSyncService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpGet]
        public ActionResult Index()
        {
            return View(new AdminImportVM { SavedRemoteSiteConfigs = _remoteConfigRepository.Table.ToList() });
        }

        [HttpPost]
        [ActionName("Index")]
        [FormValueRequired("saveConfig")]
        public ActionResult IndexPostSave(AdminImportVM vm)
        {
            _remoteConfigRepository.Create(new RemoteSiteConfigRecord
            {
                Url = vm.Url,
                Username = vm.Username,
                Password = vm.Password
            });

            _orchardServices.Notifier.Information(T("The remote site details have been saved. You can now sync with that site with a single click by using the 'Sync with this config' link next to the site config in the table below."));

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("RemoteConfig")]
        [FormValueRequired("sync")]
        public ActionResult RemoteConfigPostSync(int id) 
        {
            _orchardServices.WorkContext.HttpContext.Server.ScriptTimeout = 600;

            var result = _contentSyncService.Sync(id);

            switch (result.Status)
            {
                case ContentSyncResultStatus.RemoteSiteConfigDoesNotExist:
                    _orchardServices.Notifier.Warning(T("The site details you attempted to sync with no longer exist."));
                    break;
                case ContentSyncResultStatus.RemoteUrlTimedout:
                    _orchardServices.Notifier.Error(T("There was an unexpected error when trying to export the remote site. The remote site was not accessible."));
                    break;
                case ContentSyncResultStatus.RemoteUrlUnauthorized:
                    _orchardServices.Notifier.Warning(T("Either the username and password you supplied is incorrect, or this user does not have the correct permissions to export content."));
                    break;
                case ContentSyncResultStatus.RemoteUrlFailed:
                    _orchardServices.Notifier.Warning(T("The remote site failed to return an export of its content."));
                    break;
                case ContentSyncResultStatus.RecipeExecutionPending:
                    _orchardServices.Notifier.Information(T("The remote site's content export has been downloaded and is now in the process of being imported."));
                    break;
                case ContentSyncResultStatus.RecipeExecutionFailed:
                    _orchardServices.Notifier.Error(T("The remote site's content export has been downloaded, but there was an error when trying to import the content."));
                    return RedirectToAction("ImportResult", "Admin", new { ExecutionId = result.RecipeExecutionId, area = "Orchard.ImportExport" });
                case ContentSyncResultStatus.OK:
                    _orchardServices.Notifier.Information(T("Site content has been synced."));
                    break;
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("RemoteConfig")]
        [FormValueRequired("delete")]
        public ActionResult RemoteConfigPostDelete(int id)
        {
            _remoteConfigRepository.Delete(new RemoteSiteConfigRecord{Id = id});

            _orchardServices.Notifier.Information(T("The selected remote site details have been removed."));

            return RedirectToAction("Index");
        }
    }
}