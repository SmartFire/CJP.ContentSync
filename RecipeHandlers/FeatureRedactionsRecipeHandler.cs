using System;
using System.Linq;
using CJP.ContentSync.ExtensionMethods;
using CJP.ContentSync.Models;
using CJP.ContentSync.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace CJP.ContentSync.RecipeHandlers {
    public class FeatureRedactionsRecipeHandler : IRecipeHandler {
        private readonly IFeatureRedactionService _featureRedactionService;

        public FeatureRedactionsRecipeHandler(IFeatureRedactionService featureRedactionService)
        {
            _featureRedactionService = featureRedactionService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        /*  
         <FeatureRedactions>
          <add FeatureId="Orchard.Azure" Enabled="true" />
         </FeatureRedactions>
        */
        // Add a set of redactions
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "FeatureRedactions", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var redactions = recipeContext.RecipeStep.Step.Descendants().Where(f => f.Name == "add");
            foreach (var redaction in redactions)
            {
                var featureId = redaction.Attribute("FeatureId").Value;
                var enabled = String.Equals(redaction.Attribute("Enabled").Value, "true", StringComparison.OrdinalIgnoreCase);

                _featureRedactionService.AddRedaction(new FeatureRedactionRecord { FeatureId = featureId, Enabled = enabled});
            }

            recipeContext.Executed = true;
        }
    }
}
