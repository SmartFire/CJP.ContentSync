using System;
using System.Xml;
using System.Xml.Linq;
using CJP.ContentSync.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Settings;

namespace CJP.ContentSync.RecipeHandlers {
    //This handler is similar to the standard settings import handler taken from Orchard.Recipe. Main difference is the added ability to restore redacted text
    public class RedactedSettingsRecipeHandler : IRecipeHandler {
        private readonly ISiteService _siteService;
        private readonly ITextRedactionService _textRedactionService;

        public RedactedSettingsRecipeHandler(ISiteService siteService, ITextRedactionService textRedactionService)
        {
            _siteService = siteService;
            _textRedactionService = textRedactionService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        /*  
         <RedactedSiteSettings>
          <SiteSettingsPart PageSize="30" />
          <CommentSettingsPart ModerateComments="true" />
         </RedactedSiteSettings>
        */
        // Set site and part settings.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "RedactedSiteSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var site = _siteService.GetSiteSettings();
            foreach (var element in recipeContext.RecipeStep.Step.Elements()) {
                var partName = XmlConvert.DecodeName(element.Name.LocalName);
                foreach (var contentPart in site.ContentItem.Parts) {
                    if (!String.Equals(contentPart.PartDefinition.Name, partName, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }
                    foreach (var attribute in element.Attributes()) {
                        SetSetting(attribute, contentPart);
                    }
                }
            }

            recipeContext.Executed = true;
        }

        private void SetSetting(XAttribute attribute, ContentPart contentPart) {
            var attributeName = attribute.Name.LocalName;
            var attributeValue = _textRedactionService.RestoreText(attribute.Value);
            var property = contentPart.GetType().GetProperty(attributeName);
            if (property == null) {
                throw new InvalidOperationException(string.Format("Could not set setting {0} for part {1} because it was not found.", attributeName, contentPart.PartDefinition.Name));
            }
            var propertyType = property.PropertyType;
            if (propertyType == typeof(string)) {
                property.SetValue(contentPart, attributeValue, null);
            }
            else if (propertyType == typeof(bool)) {
                property.SetValue(contentPart, Boolean.Parse(attributeValue), null);
            }
            else if (propertyType == typeof(int)) {
                property.SetValue(contentPart, Int32.Parse(attributeValue), null);
            }
            else {
                throw new InvalidOperationException(string.Format("Could not set setting {0} for part {1} because its type is not supported. Settings should be integer, boolean or string.", attributeName, contentPart.PartDefinition.Name));
            }
        }
    }
}
