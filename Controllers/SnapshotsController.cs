using System.Linq;
using System.Web.Mvc;
using CJP.ContentSync.Models;
using CJP.ContentSync.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Services;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace CJP.ContentSync.Controllers
{
    [Admin]
    public class SnapshotsController : Controller
    {
        private readonly ISnapshotService _snapshotService;
        private readonly INotifier _notifier;
        private readonly IClock _clock;
        private readonly IContentExportService _contentExportService;

        public SnapshotsController(ISnapshotService snapshotService, INotifier notifier, IClock clock, IContentExportService contentExportService)
        {
            _snapshotService = snapshotService;
            _notifier = notifier;
            _clock = clock;
            _contentExportService = contentExportService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpGet]
        public ActionResult Index()
        {
            return View(_snapshotService.GetSnaphots().OrderByDescending(ss => ss.TimeTaken));
        }

        [HttpGet]
        public ActionResult View(int? id = null) 
        {
            SnapshotRecord record = null;

            if (id.HasValue) {
                record = _snapshotService.GetSnaphot(id.Value);
            }

            if (record == null) {
                record = new SnapshotRecord {
                    TimeTaken = _clock.UtcNow,
                    Data = _contentExportService.GetContentExportFilePath()
                };
            }

            return View(record);
        }

        [HttpPost]
        public ActionResult Take()
        {
            _snapshotService.TakeSnaphot();
            _notifier.Information(T("A snapshot of this site's current configuration has been taken and added to the collection."));

            return RedirectToAction("Index");
        }
    }
}