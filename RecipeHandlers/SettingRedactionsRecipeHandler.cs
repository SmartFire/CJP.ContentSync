using System;
using System.Linq;
using CJP.ContentSync.Models;
using CJP.ContentSync.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace CJP.ContentSync.RecipeHandlers {
    public class SettingRedactionsRecipeHandler : IRecipeHandler {
        private readonly ISettingRedactionService _settingRedactionService;

        public SettingRedactionsRecipeHandler(ISettingRedactionService settingRedactionService)
        {
            _settingRedactionService = settingRedactionService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        /*  
         <SettingRedactions>
          <add Setting="SiteSettingsPart.SiteOwner" Value="admin" />
         </SettingRedactions>
        */
        // Add a set of redactions
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "SettingRedactions", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var redactions = recipeContext.RecipeStep.Step.Descendants().Where(f => f.Name == "add");
            foreach (var redaction in redactions)
            {
                var setting = redaction.Attribute("Setting").Value;
                var value = redaction.Attribute("Value").Value;

                _settingRedactionService.AddRedaction(new SettingRedactionRecord { SettingName = setting, Value = value});
            }

            recipeContext.Executed = true;
        }
    }
}
