﻿using System.Web.Mvc;
using CJP.ContentSync.Models;
using CJP.ContentSync.Services;
using Orchard;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Logging;
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

        public AdminController(IImportExportService importExportService, IOrchardServices orchardServices, IContentExportService contentExportService, IRecipeJournal recipeJournal) {
            _importExportService = importExportService;
            _orchardServices = orchardServices;
            _contentExportService = contentExportService;
            _recipeJournal = recipeJournal;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpGet]
        public ActionResult Index()
        {
            return View(new AdminImportVM { Url = "http://localhost:30321/orchardlocal", Username="OrchardAdmin", Password = "Password"});
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult IndexPost(AdminImportVM vm) {
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

            if (journal.Status == RecipeStatus.Started) {
                _orchardServices.Notifier.Information(T("Site content is in the process of bein synced, but has not yet completed. You can refresh this page to monitor the progress of your sync"));
            }
            else
            {
                _orchardServices.Notifier.Warning(T("The import from the remote site failed"));
            }

            return RedirectToAction("ImportResult", "Admin", new { ExecutionId = executionId, area="Orchard.ImportExport" });
        }
    }
}