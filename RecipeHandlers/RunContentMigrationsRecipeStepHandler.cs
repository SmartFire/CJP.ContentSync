using System;
using CJP.ContentSync.ExtensionMethods;
using CJP.ContentSync.Services;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace CJP.ContentSync.RecipeHandlers
{
    public class RunContentMigrationsRecipeStepHandler : IRecipeHandler
    {
        private readonly IContentMigrationManager _contentMigrationManager;
        private readonly IRealtimeFeedbackService _realtimeFeedbackService;

        public RunContentMigrationsRecipeStepHandler(IContentMigrationManager contentMigrationManager, IRealtimeFeedbackService realtimeFeedbackService) {
            _contentMigrationManager = contentMigrationManager;
            _realtimeFeedbackService = realtimeFeedbackService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /*
          <RunContentMigrations />
         */
        //Run any pending content migrations
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "RunContentMigrations", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            _realtimeFeedbackService.Info(T("Starting 'Run Content Migrations' step"));

            _contentMigrationManager.ExecutePendingMigrations();

            _realtimeFeedbackService.Info(T("Step 'Run Content Migrations' has finished"));
            recipeContext.Executed = true;
        }
    }
}