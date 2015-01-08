using System.Web.Mvc;
using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using CJP.ContentSync.Models.ViewModels;
using CJP.ContentSync.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace CJP.ContentSync.Controllers
{
    [Admin]
    public class RedactionsController : Controller
    {
        private readonly IContentRedactionService _textRedactionService;
        private readonly ISettingRedactionService _settingRedactionService;
        private readonly IFeatureRedactionService _featureRedactionService;
        private readonly INotifier _notifier;

        public RedactionsController(IContentRedactionService textRedactionService, ISettingRedactionService settingRedactionService, IFeatureRedactionService featureRedactionService, INotifier notifier)
        {
            _textRedactionService = textRedactionService;
            _settingRedactionService = settingRedactionService;
            _featureRedactionService = featureRedactionService;
            _notifier = notifier;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpGet]
        public ActionResult Index() {
            var vm = new RedactionsIndexVM {
                ContentRedactions = _textRedactionService.GetRedactions(),
                FeatureRedactions = _featureRedactionService.GetRedactions(),
                SettingRedactions = _settingRedactionService.GetRedactions(),
            };

            return View(vm);
        }

        public ActionResult EditContentRedaction(int id = 0)
        {
            var redaction = _textRedactionService.GetRedaction(id) ?? new RedactionRecord();

            return View(redaction);
        }

        [HttpPost, ActionName("EditContentRedaction"), FormValueRequired("save")]
        public ActionResult EditContentRedactionPost(RedactionRecord redaction, int id = 0)
        {
            redaction.Id = id;
            var status = _textRedactionService.UpdateRedaction(redaction);

            switch (status)
            {
                case RedactionOperationStatus.Created:
                    _notifier.Information(T("Redaction created"));
                    break;
                case RedactionOperationStatus.Updated:
                    _notifier.Information(T("Redaction updated"));
                    break;
                case RedactionOperationStatus.NotUnique:
                    _notifier.Error(T("Redaction could not be saved because the Placeholder was not unique. Ensure that the placeholder you choose does not already exist for another redaction."));
                    return View(redaction);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("EditContentRedaction"), FormValueRequired("delete")]
        public ActionResult EditSettingRedactionDelete(RedactionRecord redaction, int id = 0)
        {
            redaction.Id = id;
            _textRedactionService.DeleteRedaction(redaction);

            _notifier.Information(T("Content Redaction deleted"));

            return RedirectToAction("Index");
        }


        public ActionResult EditSettingRedaction(int id = 0)
        {
            var redaction = _settingRedactionService.GetRedaction(id) ?? new SettingRedactionRecord();

            return View(redaction);
        }

        [HttpPost, ActionName("EditSettingRedaction"), FormValueRequired("save")]
        public ActionResult EditSettingRedactionPost(SettingRedactionRecord redaction, int id = 0)
        {
            redaction.Id = id;
            var status = _settingRedactionService.UpdateRedaction(redaction);

            switch (status)
            {
                case RedactionOperationStatus.Created:
                    _notifier.Information(T("Setting Redaction created"));
                    break;
                case RedactionOperationStatus.Updated:
                    _notifier.Information(T("Setting Redaction updated"));
                    break;
                case RedactionOperationStatus.NotUnique:
                    _notifier.Error(T("Setting Redaction could not be saved because there is already a Setting Redaction for the setting {0}.", redaction.SettingName));
                    return View(redaction);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("EditSettingRedaction"), FormValueRequired("delete")]
        public ActionResult EditSettingRedactionDelete(SettingRedactionRecord redaction, int id = 0)
        {
            redaction.Id = id;
            _settingRedactionService.DeleteRedaction(redaction);

            _notifier.Information(T("Setting Redaction deleted"));

            return RedirectToAction("Index");
        }


        public ActionResult EditFeatureRedaction(int id = 0)
        {
            var redaction = _featureRedactionService.GetRedaction(id) ?? new FeatureRedactionRecord();

            return View(redaction);
        }

        [HttpPost, ActionName("EditFeatureRedaction"), FormValueRequired("save")]
        public ActionResult EditFeatureRedactionPost(FeatureRedactionRecord redaction, int id = 0)
        {
            redaction.Id = id;
            var status = _featureRedactionService.UpdateRedaction(redaction);

            switch (status)
            {
                case RedactionOperationStatus.Created:
                    _notifier.Information(T("Feature Redaction created"));
                    break;
                case RedactionOperationStatus.Updated:
                    _notifier.Information(T("Feature Redaction updated"));
                    break;
                case RedactionOperationStatus.NotUnique:
                    _notifier.Error(T("Feature Redaction could not be saved because there is already a feature redaction for the feature {0}.", redaction.FeatureId));
                    return View(redaction);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("EditFeatureRedaction"), FormValueRequired("delete")]
        public ActionResult EditFeatureRedactionDelete(FeatureRedactionRecord redaction, int id = 0)
        {
            redaction.Id = id;
            _featureRedactionService.DeleteRedaction(redaction);

            _notifier.Information(T("Feature Redaction deleted"));

            return RedirectToAction("Index");
        }
    }
}