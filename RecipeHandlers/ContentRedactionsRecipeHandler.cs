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
    public class ContentRedactionsRecipeHandler : IRecipeHandler {
        private readonly IContentRedactionService _textRedactionService;
        private readonly IRealtimeFeedbackService _realtimeFeedbackService;

        public ContentRedactionsRecipeHandler(IContentRedactionService textRedactionService, IRealtimeFeedbackService realtimeFeedbackService)
        {
            _textRedactionService = textRedactionService;
            _realtimeFeedbackService = realtimeFeedbackService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        /*  
         <ContentRedactions>
          <add Regex="Production" Placeholder="EnvironmentName" ReplaceWith="Local" />
         </ContentRedactions>
        */
        // Add a set of redactions
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "ContentRedactions", StringComparison.OrdinalIgnoreCase)
                && !String.Equals(recipeContext.RecipeStep.Name, "Redactions", StringComparison.OrdinalIgnoreCase))//this check is for legacy reasons
            {
                return;
            }
            _realtimeFeedbackService.Info(T("Starting 'Content Redactions' step"));

            var redactions = recipeContext.RecipeStep.Step.Descendants().Where(f => f.Name == "add");
            foreach (var redaction in redactions)
            {
                var placeholder = redaction.Attribute("Placeholder").Value;
                var regex = redaction.Attribute("Regex").Value;
                var replaceWith = redaction.Attribute("ReplaceWith").Value;

                _realtimeFeedbackService.Info(T("Adding content redaction {0} to match regex {1} and relace with {2}", placeholder, regex, replaceWith));
                _textRedactionService.AddRedaction(new RedactionRecord { Placeholder = placeholder, Regex = regex, ReplaceWith = replaceWith, });
            }

            _realtimeFeedbackService.Info(T("Step 'Content Redactions' has finished"));
            recipeContext.Executed = true;
        }
    }
}
