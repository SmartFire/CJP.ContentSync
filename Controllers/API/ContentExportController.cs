﻿using System.Net;
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
        private readonly IAuthorizationService _authorizationService;

        public ContentExportController(IContentExportService contentExportService, IMembershipService membershipService, IAuthenticationService authenticationService, IAuthorizationService authorizationService)
        {
            _contentExportService = contentExportService;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
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

            if (!_authorizationService.TryCheckAccess(ApiPermissions.ContentExportApi, user, null)){
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                Response.End();
                return new HttpUnauthorizedResult();
            }


            var filePath = _contentExportService.GetContentExportText();

            return File(filePath, "text/xml", "export.xml");
        }
    }
}