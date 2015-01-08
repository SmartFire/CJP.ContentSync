﻿using System.Linq;
using CJP.ContentSync.Models;
using CJP.ContentSync.Services;
using Orchard.Commands;
using Orchard.Data;
using Orchard.ImportExport.Services;
using Orchard.Recipes.Services;

namespace CJP.ContentSync.Commands {
    public class ContentMigrationCommands : DefaultOrchardCommandHandler {
        private readonly IContentMigrationManager _contentMigrationManager;
        private readonly IContentExportService _contentExportService;
        private readonly IImportExportService _importExportService;
        private readonly IRecipeJournal _recipeJournal;
        private readonly IRepository<RemoteSiteConfigRecord> _repository;

        public ContentMigrationCommands(IContentMigrationManager contentMigrationManager, IContentExportService contentExportService, IImportExportService importExportService, IRecipeJournal recipeJournal, IRepository<RemoteSiteConfigRecord> repository)
        {
            _contentMigrationManager = contentMigrationManager;
            _contentExportService = contentExportService;
            _importExportService = importExportService;
            _recipeJournal = recipeJournal;
            _repository = repository;
        }


        [OrchardSwitch]
        public string Url { get; set; }

        [OrchardSwitch]
        public string Username { get; set; }

        [OrchardSwitch]
        public string Password { get; set; }


        [CommandName("CJP contentsync run migrations")]
        [CommandHelp("CJP contentsync run migrations\r\n\t" + "Runs any pending content migrations")]
        public void RunMigrations()
        {

            var result = _contentMigrationManager.ExecutePendingMigrations().ToList();

            foreach (var migration in result.SelectMany(m => m.SuccessfulMigrations))
            {
                Context.Output.WriteLine(T("Migration '{0}' successfully executed", migration));
            }

            foreach (var migration in result.SelectMany(m => m.FailedMigrations))
            {
                Context.Output.WriteLine(T("Migration '{0}' failed", migration));
            }

            Context.Output.WriteLine(T("Messages:"));
            foreach (var message in result.SelectMany(r => r.Messages))
            {
                Context.Output.WriteLine(T(message));
            }
        }


        [CommandName("CJP contentsync from remote")]
        [CommandHelp("CJP contentsync from remote\r\n\t" + "Gets an export from the remote site and syncs this site with it")]
        [OrchardSwitches("Url,Username,Password")]
        public void RemoteSyncWithCredentials()
        {
            var result = _contentExportService.GetContentExportFromUrl(Url, Username, Password);


            if (result.Status == ApiResultStatus.Unauthorized)
            {
                Context.Output.WriteLine(T("Either the username and password you supplied is incorrect, or this user does not have the correct permissions to export content"));
                return;
            }

            if (result.Status == ApiResultStatus.Failed)
            {
                Context.Output.WriteLine(T("There was an unexpected error when trying to export the remote site"));
                return;
            }

            Context.Output.WriteLine(T("Site content and configurations have been downloaded and will now be imported"));
            var executionId = _importExportService.Import(result.Text);

            _recipeJournal.GetRecipeJournal(executionId);
        }


        [CommandName("CJP contentsync from saved config")]
        [CommandHelp("CJP contentsync from saved config\r\n\t" + "Gets an export from the remote site and syncs this site with it")]
        [OrchardSwitches("Url")]
        public void RemoteSync() {
            var record = _repository.Fetch(c => c.Url == Url).FirstOrDefault();
            if (record == null)
            {
                Context.Output.WriteLine(T("Could not sync with the remote site {0} because there is no saved remote site config with the url {0}", Url));

                return;
            }

            var result = _contentExportService.GetContentExportFromUrl(record.Url, record.Username, record.Password);

            if (result.Status == ApiResultStatus.Unauthorized)
            {
                Context.Output.WriteLine(T("Either the username and password you supplied is incorrect, or this user does not have the correct permissions to export content"));
                return;
            }

            if (result.Status == ApiResultStatus.Failed)
            {
                Context.Output.WriteLine(T("There was an unexpected error when trying to export the remote site"));
                return;
            }

            Context.Output.WriteLine(T("Site content and configurations have been downloaded and will now be imported"));
            var executionId = _importExportService.Import(result.Text);

            _recipeJournal.GetRecipeJournal(executionId);
        }
    }
}