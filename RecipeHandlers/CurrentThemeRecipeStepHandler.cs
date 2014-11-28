using System;
using CJP.ContentSync.ExtensionMethods;
using CJP.ContentSync.Services;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Themes.Services;

namespace CJP.ContentSync.RecipeHandlers
{
    public class CurrentThemeRecipeStepHandler : IRecipeHandler
    {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IRealtimeFeedbackService _realtimeFeedbackService;

        public CurrentThemeRecipeStepHandler(ISiteThemeService siteThemeService, IRealtimeFeedbackService realtimeFeedbackService) {
            _siteThemeService = siteThemeService;
            _realtimeFeedbackService = realtimeFeedbackService;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        /*
            <CurrentTheme name="MySuperTheme" />
         */
        //Enable any features that are in the list, disable features that aren't in the list
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "CurrentTheme", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _realtimeFeedbackService.Info(T("Entering the 'Current Theme' step"));

            var themeToEnable = recipeContext.RecipeStep.Step.Attribute("id").Value;
            _realtimeFeedbackService.Info(T("Setting the current theme to {0}", themeToEnable));

            _siteThemeService.SetSiteTheme(themeToEnable);

            _realtimeFeedbackService.Info(T("The current theme has been set to {0}", themeToEnable));
            recipeContext.Executed = true;
        }
    }
}