using System.Net;
using System.Web.Mvc;
using CJP.ContentSync.Permissions;
using CJP.ContentSync.Services;
using Orchard.Security;

namespace CJP.ContentSync.Controllers.API
{
    public class ContentExportController : Controller {
        private readonly IContentExportService _contentExportService;
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizer _authorizer;

        public ContentExportController(IContentExportService contentExportService, IMembershipService membershipService, IAuthenticationService authenticationService, IAuthorizer authorizer)
        {
            _contentExportService = contentExportService;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _authorizer = authorizer;
        }


        [HttpGet]
        public ActionResult Index(string username, string password) {
            var user = _membershipService.ValidateUser(username, password);

            if (user == null) {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                Response.End();
                return new HttpUnauthorizedResult();
            }

            _authenticationService.SignIn(user, false);

            if (!_authorizer.Authorize(ContentSyncPermissions.ContentExportApi)){
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                Response.End();
                return new HttpUnauthorizedResult();
            }


            var filePath = _contentExportService.GetContentExportFilePath();

            return File(filePath, "text/xml", "export.xml");
        }
    }
}