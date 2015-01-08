using System;
using System.Linq;
using CJP.ContentSync.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace CJP.ContentSync.RecipeHandlers {
    public class RemoteSiteConfigRecipeHandler : IRecipeHandler {
        private readonly IRepository<RemoteSiteConfigRecord> _repository;

        public RemoteSiteConfigRecipeHandler(IRepository<RemoteSiteConfigRecord> repository)
        {
            _repository = repository;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        /*  
         <RemoteSiteConfigs>
          <add Url="http://www.example.com" Username="YourContentSyncUser" Password="YourSuperSecurePassword" />
         </RemoteSiteConfigs>
        */
        // Add a set of redactions
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "RemoteSiteConfigs", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var redactions = recipeContext.RecipeStep.Step.Descendants().Where(f => f.Name == "add");
            foreach (var redaction in redactions)
            {
                var url = redaction.Attribute("Url").Value;
                var username = redaction.Attribute("Username").Value;
                var password = redaction.Attribute("Password").Value;

                _repository.Create(new RemoteSiteConfigRecord{Url = url, Username = username, Password = password});
            }

            recipeContext.Executed = true;
        }
    }
}
