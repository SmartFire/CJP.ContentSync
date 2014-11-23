using System;
using System.Linq;
using CJP.ContentSync.Models;
using CJP.ContentSync.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace CJP.ContentSync.RecipeHandlers {
    public class RedactionsRecipeHandler : IRecipeHandler {
        private readonly ITextRedactionService _textRedactionService;

        public RedactionsRecipeHandler(ITextRedactionService textRedactionService)
        {
            _textRedactionService = textRedactionService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        /*  
         <Redactions>
          <add Regex="Production" Placeholder="EnvironmentName" ReplaceWith="Local" />
         </Redactions>
        */
        // Add a set of redactions
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Redactions", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var redactions = recipeContext.RecipeStep.Step.Descendants().Where(f => f.Name == "add");
            foreach (var redaction in redactions)
            {
                _textRedactionService.AddRedaction(new RedactionRecord { Placeholder = redaction.Attribute("Placeholder").Value, Regex = redaction.Attribute("Regex").Value, ReplaceWith = redaction.Attribute("ReplaceWith").Value, });
            }

            recipeContext.Executed = true;
        }
    }
}
