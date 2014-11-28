using System;
using System.Linq;
using CJP.ContentSync.ExtensionMethods;
using CJP.ContentSync.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Widgets.Models;

namespace CJP.ContentSync.RecipeHandlers
{
    public class DeleteContentRecipeStepHandler : IRecipeHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IRealtimeFeedbackService _realtimeFeedbackService;

        public DeleteContentRecipeStepHandler(IContentManager contentManager, IRealtimeFeedbackService realtimeFeedbackService) {
            _contentManager = contentManager;
            _realtimeFeedbackService = realtimeFeedbackService;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        /*
          <Delete>
            <Content Id="123456789" />
            <Layer Name="Mobile Homepage" />
         */
        //delete content and layers
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _realtimeFeedbackService.Info(T("Entering the 'Delete' step. This step will remove content as specified"));

            var items = recipeContext.RecipeStep.Step.Descendants().ToList();

            foreach (var item in items) {
                switch (item.Name.ToString().ToLowerInvariant()) //to consider: we could inject in a collection of interfaces to handle these items to allow for extensibility
                {
                    case "content":
                        var identifier = item.Attribute("Id").Value;
                        var content = _contentManager.Query<IdentityPart, IdentityPartRecord>().Where(c => c.Identifier == identifier).List().FirstOrDefault();
                        if (content != null)
                        {
                            _realtimeFeedbackService.Info(T("Removing content item '{0}'", content.ContentItem.GetContentName()));
                            _contentManager.Remove(content.ContentItem);    
                        }

                        break;
                    case "layer":
                        var name = item.Attribute("Name").Value;
                        var layer = _contentManager.Query<LayerPart, LayerPartRecord>().Where(l => l.Name == name).List().FirstOrDefault();
                        if (layer != null)
                        {
                            _realtimeFeedbackService.Info(T("Removing layer '{0}'", layer.ContentItem.GetContentName()));
                            _contentManager.Remove(layer.ContentItem);    
                        }

                        break;
                }
            }

            _realtimeFeedbackService.Info(T("The 'Delete' step is finished; it removed {0} content items.", items.Count()));
            recipeContext.Executed = true;
        }
    }
}