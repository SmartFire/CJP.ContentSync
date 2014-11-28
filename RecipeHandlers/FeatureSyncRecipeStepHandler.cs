using System;
using System.Linq;
using CJP.ContentSync.ExtensionMethods;
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
        private readonly IRealtimeFeedbackService _realtimeFeedbackService;

        public FeatureSyncRecipeStepHandler(IFeatureManager featureManager, IRealtimeFeedbackService realtimeFeedbackService) {
            _featureManager = featureManager;
            _realtimeFeedbackService = realtimeFeedbackService;

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

            _realtimeFeedbackService.Info(T("Entering the 'Feature Sync' step. This step ensures that the features enabled and disabled locally match those enabled and disabled remotely."));

            var features = recipeContext.RecipeStep.Step.Descendants();
            var featureIds = features.Where(f => f.Name == "Feature").Select(f => f.Attribute("Id").Value).ToList();
            _realtimeFeedbackService.Info(T("The remote site has {0} features enabled.", featureIds.Count()));

            var availableFeatures = _featureManager.GetAvailableFeatures();
            var enabledFeatures = _featureManager.GetEnabledFeatures().ToList();

            var featuresToDisable = enabledFeatures.Where(f => !featureIds.Contains(f.Id)).Select(f => f.Id).ToList();
            _realtimeFeedbackService.Info(T("{0} feature(s) are due to be disabled:", featuresToDisable.Count()));
            foreach (var featureToDisable in featuresToDisable)
            {
                _realtimeFeedbackService.Info(T(featureToDisable));
            }

            var featuresToEnable = availableFeatures
                .Where(f => featureIds.Contains(f.Id)) //available features that are in the list of features that need to be enabled
                .Where(f => !enabledFeatures.Select(ef => ef.Id).Contains(f.Id)) //remove features that are already enabled
                .Select(f => f.Id)
                .ToList();
            _realtimeFeedbackService.Info(T("{0} feature(s) are due to be enabled.", featuresToEnable.Count()));
            foreach (var featureToEnable in featuresToEnable)
            {
                _realtimeFeedbackService.Info(T(featureToEnable));
            }

            _realtimeFeedbackService.Info(T("Disabling the following features: ", string.Join(", ", featuresToDisable)));
            _featureManager.DisableFeatures(featuresToDisable, true);

            _realtimeFeedbackService.Info(T("Enabling the following features: ", string.Join(", ", featuresToEnable)));
            _featureManager.EnableFeatures(featuresToEnable, true);

            _realtimeFeedbackService.Info(T("Feature Sync has completed"));
            recipeContext.Executed = true;
        }
    }
}