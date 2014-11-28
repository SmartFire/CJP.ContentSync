using System.Linq;
using System.Web.Mvc;
using CJP.ContentSync.Models;
using CJP.ContentSync.Services;
using Orchard;
using Orchard.Data;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.UI.Notify;

namespace CJP.ContentSync.Controllers
{
    public class AdminController : Controller
    {
        private readonly IImportExportService _importExportService;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentExportService _contentExportService;
        private readonly IRecipeJournal _recipeJournal;
        private readonly IRepository<RemoteSiteConfigRecord> _remoteConfigRepository;

        public AdminController(IImportExportService importExportService, IOrchardServices orchardServices, IContentExportService contentExportService, IRecipeJournal recipeJournal, IRepository<RemoteSiteConfigRecord> remoteConfigRepository) {
            _importExportService = importExportService;
            _orchardServices = orchardServices;
            _contentExportService = contentExportService;
            _recipeJournal = recipeJournal;
            _remoteConfigRepository = remoteConfigRepository;

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
        [FormValueRequired("syncContent")]
        public ActionResult IndexPost(AdminImportVM vm)
        {
            // sets the setup request timeout to 10 minutes to give enough time to execute custom recipes.  
            HttpContext.Server.ScriptTimeout = 600;

            var result = _contentExportService.GetContentExportFromUrl(vm.Url, vm.Username, vm.Password);

            if (result.Status == ApiResultStatus.Unauthorized)
            {
                _orchardServices.Notifier.Warning(T("Either the username and password you supplied is incorrect, or this user does not have the correct permissions to export content"));
                return View("Index", vm);
            }

            if (result.Status == ApiResultStatus.Failed)
            {
                _orchardServices.Notifier.Error(T("There was an unexpected error when trying to export the remote site"));
                return View("Index", vm);
            }

            _orchardServices.Notifier.Information(T("Site content and configurations have been downloaded and will now be imported"));
            var executionId = _importExportService.Import(result.Text);
            var journal = _recipeJournal.GetRecipeJournal(executionId);

            if (journal.Status == RecipeStatus.Complete)
            {
                _orchardServices.Notifier.Information(T("Site content has been synced"));
                return View("Index", new AdminImportVM());
            }

            if (journal.Status == RecipeStatus.Started){
                _orchardServices.Notifier.Information(T("Site content is in the process of being synced, but has not yet completed. You can refresh this page to monitor the progress of your sync"));
            }
            else
            {
                _orchardServices.Notifier.Warning(T("The import from the remote site failed"));
            }

            return RedirectToAction("ImportResult", "Admin", new { ExecutionId = executionId, area = "Orchard.ImportExport" });
        }

        [HttpPost]
        [ActionName("Index")]
        [FormValueRequired("saveConfig")]
        public ActionResult IndexPostSave(AdminImportVM vm)
        {
            _remoteConfigRepository.Create(new RemoteSiteConfigRecord
            {
                //LastSynced = null,
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
        public ActionResult RemoteConfigPostSync(int id) {
            var config = _remoteConfigRepository.Get(id);

            if (config == null)
            {
                _orchardServices.Notifier.Warning(T("The site details you attempted to sync with no longer exist."));
                return RedirectToAction("Index");
            }

            return IndexPost(new AdminImportVM {Password = config.Password, Url = config.Url, Username = config.Username});
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