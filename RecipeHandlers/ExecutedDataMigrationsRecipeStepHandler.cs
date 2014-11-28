using System;
using System.Linq;
using CJP.ContentSync.ExtensionMethods;
using CJP.ContentSync.Services;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace CJP.ContentSync.RecipeHandlers
{
    public class ExecutedDataMigrationsRecipeStepHandler : IRecipeHandler
    {
        private readonly IContentMigrationStateService _contentMigrationStateService;
        private readonly IRealtimeFeedbackService _realtimeFeedbackService;

        public ExecutedDataMigrationsRecipeStepHandler(IContentMigrationStateService contentMigrationStateService, IRealtimeFeedbackService realtimeFeedbackService) {
            _contentMigrationStateService = contentMigrationStateService;
            _realtimeFeedbackService = realtimeFeedbackService;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        /*
          <ExecutedDataMigrations>
            <Migration Name="~/Modules/CJP.ContentSync/ContentMigrations/CJP.ContentSync/TestMigration1" />
         */
        //Save any migrations that are in the list, delete migrations that aren't in the list
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "ExecutedDataMigrations", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _realtimeFeedbackService.Info(T("Entering the 'Executed Data Migrations' step. This step will reset the local record of content migrations to the state of the remote server"));

            var migrations = recipeContext.RecipeStep.Step.Descendants();
            var migrationNames = migrations.Where(f => f.Name == "Migration").Select(f => f.Attribute("Name").Value).ToList();

            _realtimeFeedbackService.Info(T("Found {0} migrations that have ran remotely:", migrationNames.Count()));
            foreach (var migrationName in migrationNames)
            {
                _realtimeFeedbackService.Info(T(migrationName));
            }

            var locallyRanMigrations = _contentMigrationStateService.GetExecutedMigrations().ToList();

            _realtimeFeedbackService.Info(T("Found {0} migrations that have ran locally:", locallyRanMigrations.Count()));
            foreach (var migrationName in locallyRanMigrations)
            {
                _realtimeFeedbackService.Info(T(migrationName));
            }

            foreach (var migration in locallyRanMigrations.Where(m => !migrationNames.Contains(m)))
            {//migrations that have been executed locally, but not in the recipe that is being executed
                _realtimeFeedbackService.Info(T("Marking migration '{0}' as pending", migration));
                _contentMigrationStateService.MarkMigrationAsPending(migration);
            }

            foreach (var migrationToAdd in migrationNames.Where(m => !locallyRanMigrations.Contains(m)))
            {
                _realtimeFeedbackService.Info(T("Marking migration '{0}' as executed", migrationToAdd));
                _contentMigrationStateService.MarkMigrationAsExecuted(migrationToAdd);
            }

            _realtimeFeedbackService.Info(T("The 'Executed Data Migrations' step is finished"));
            recipeContext.Executed = true;
        }
    }
}