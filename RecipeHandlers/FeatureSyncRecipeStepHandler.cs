using System;
using System.Linq;
using CJP.ContentSync.Services;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace CJP.ContentSync.RecipeHandlers
{
    public class FeatureSyncRecipeStepHandler : IRecipeHandler
    {
        private readonly IFeatureManager _featureManager;
        private readonly IFeatureRedactionService _featureRedactionService;

        public FeatureSyncRecipeStepHandler(IFeatureManager featureManager, IFeatureRedactionService featureRedactionService) {
            _featureManager = featureManager;
            _featureRedactionService = featureRedactionService;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        /*
          <EnabledFeatures>
            <Feature Id="Orchard.ImportExport" />
         */
        //Enable any features that are in the list, disable features that aren't in the list
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "FeatureSync", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var features = recipeContext.RecipeStep.Step.Descendants();
            var featureIds = features.Where(f => f.Name == "Feature").Select(f => f.Attribute("Id").Value).ToList();

            //we now have the list of features that are enabled on the remote site
            //next thing to do is add and remove features to and from this list based on our feature redactions
            var featureRedactions = _featureRedactionService.GetRedactions().ToList();

            featureIds.AddRange(featureRedactions.Where(r => r.Enabled).Select(r => r.FeatureId)); //adding features that need to be enabled
            featureIds.RemoveAll(f => featureRedactions.Where(r => !r.Enabled).Select(r => r.FeatureId).Contains(f)); //removing features that need to be disabled

            //adding redactions may have caused duplicity
            featureIds = featureIds.Distinct().ToList();

            var availableFeatures = _featureManager.GetAvailableFeatures();
            var enabledFeatures = _featureManager.GetEnabledFeatures().ToList();

            var featuresToDisable = enabledFeatures.Where(f => !featureIds.Contains(f.Id)).Select(f => f.Id).ToList();
            var featuresToEnable = availableFeatures
                .Where(f => featureIds.Contains(f.Id)) //available features that are in the list of features that need to be enabled
                .Where(f => !enabledFeatures.Select(ef => ef.Id).Contains(f.Id)) //remove features that are already enabled
                .Select(f => f.Id)
                .ToList();

            _featureManager.DisableFeatures(featuresToDisable, true);
            _featureManager.EnableFeatures(featuresToEnable, true);

            recipeContext.Executed = true;
        }
    }
}