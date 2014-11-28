using System;
using System.Linq;
using CJP.ContentSync.ExtensionMethods;
using CJP.ContentSync.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace CJP.ContentSync.RecipeHandlers
{
    public class ContentTrimRecipeStepHandler : IRecipeHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IRealtimeFeedbackService _realtimeFeedbackService;

        public ContentTrimRecipeStepHandler(IContentManager contentManager, IRealtimeFeedbackService realtimeFeedbackService) {
            _contentManager = contentManager;
            _realtimeFeedbackService = realtimeFeedbackService;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        /* <ContentTrim>
         *   <ContentTypes>
         *      <add type="page"/>
         *      <add type="widget"/>
         *   </ContentTypes>
         *   <ContentToKeep>
         *      <add identifier="123456789"/>
         *      <add identifier="321654897"/>
         *   </ContentType>
         *  </ContentTrim>
         */
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "ContentTrim", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _realtimeFeedbackService.Info(T("Entering the 'Content Trim' step"));

            var contentTypesElement = recipeContext.RecipeStep.Step.Descendants("ContentTypes").FirstOrDefault();
            var contentToKeepElement = recipeContext.RecipeStep.Step.Descendants("ContentToKeep").FirstOrDefault();

            if (contentTypesElement == null) 
            {
                _realtimeFeedbackService.Error(T("Could not execute the Content Trim step as there was no 'ContentTypes' element in the step"));

                return;
            }
            if (contentToKeepElement == null)
            {
                _realtimeFeedbackService.Error(T("Could not execute the Content Trim step as there was no 'ContentToKeep' element in the step"));

                return;
            }

            var contentTypes = contentTypesElement.Descendants("add").Select(e => e.Attribute("type").Value).ToList();
            _realtimeFeedbackService.Info(T("The following content types will be synced:"));

            foreach (var contentType in contentTypes)
            {
                _realtimeFeedbackService.Info(T(contentType));
            }

            var contentToKeep = contentToKeepElement.Descendants("add").Select(e => e.Attribute("identifier").Value).ToList();
            _realtimeFeedbackService.Info(T("Identified {0} pieces of content from the remote site", contentTypes.Count()));

            var contentItems = _contentManager.Query<IdentityPart, IdentityPartRecord>(contentTypes.ToArray()).Where(c => !contentToKeep.Contains(c.Identifier)).List().ToList();
            _realtimeFeedbackService.Info(T("Identified {0} pieces of content that exist in the local site, but not in the remote site. This content will be removed.", contentItems.Count()));

            foreach (var contentItem in contentItems)
            {
                _realtimeFeedbackService.Info(T("Removing {0} '{2}' (ID: {1})", contentItem.ContentItem.ContentType, contentItem.ContentItem.Id, contentItem.ContentItem.GetContentName()));
                _contentManager.Remove(contentItem.ContentItem);
            }

            _realtimeFeedbackService.Info(T("The 'Content Trim' step has successfully executed"));
            recipeContext.Executed = true;
        }
    }
}