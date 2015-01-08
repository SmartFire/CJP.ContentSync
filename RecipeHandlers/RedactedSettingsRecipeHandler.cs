using System;
using System.Xml;
using System.Xml.Linq;
using CJP.ContentSync.ExtensionMethods;
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
        private readonly ISettingRedactionService _settingRedactionService;
        private readonly IRealtimeFeedbackService _realtimeFeedbackService;

        public RedactedSettingsRecipeHandler(ISiteService siteService, ISettingRedactionService settingRedactionService, IRealtimeFeedbackService realtimeFeedbackService)
        {
            _siteService = siteService;
            _settingRedactionService = settingRedactionService;
            _realtimeFeedbackService = realtimeFeedbackService;
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

            _realtimeFeedbackService.Info(T("Entering the 'Redacted Site Settings' step"));

            var site = _siteService.GetSiteSettings();
            foreach (var element in recipeContext.RecipeStep.Step.Elements()) {
                var partName = XmlConvert.DecodeName(element.Name.LocalName);
                foreach (var contentPart in site.ContentItem.Parts) {
                    if (!String.Equals(contentPart.PartDefinition.Name, partName, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }
                    foreach (var attribute in element.Attributes()){
                        SetSetting(attribute, contentPart);
                    }
                }
            }

            _realtimeFeedbackService.Info(T("Site settings have been updated"));
            recipeContext.Executed = true;
        }

        private void SetSetting(XAttribute attribute, ContentPart contentPart) {
            var attributeName = attribute.Name.LocalName;
            var attributeValue = _settingRedactionService.GetSettingValue(contentPart.PartDefinition.Name, attributeName, attribute.Value);

            _realtimeFeedbackService.Info(T("Updating site settings for {0}.{1}. Value being set to {2} ({3} before redactions)", contentPart.PartDefinition.Name, attributeName, attributeValue, attribute.Value));
            var property = contentPart.GetType().GetProperty(attributeName);
            if (property == null) {
                _realtimeFeedbackService.Warn(T("Could not set setting {0} for part {1} because it was not found.", attributeName, contentPart.PartDefinition.Name));

                return;
            }
            var propertyType = property.PropertyType;
            if (propertyType == typeof(string))
            {
                property.SetValue(contentPart, attributeValue, null);
                _realtimeFeedbackService.Info(T("Setting {0} for part {1} has been set to {2}.", attributeName, contentPart.PartDefinition.Name, attributeValue));
            }
            else if (propertyType == typeof(bool)) {
                property.SetValue(contentPart, Boolean.Parse(attributeValue), null);
                _realtimeFeedbackService.Info(T("Setting {0} for part {1} has been set to {2}.", attributeName, contentPart.PartDefinition.Name, attributeValue));
            }
            else if (propertyType == typeof(int)) {
                property.SetValue(contentPart, Int32.Parse(attributeValue), null);
                _realtimeFeedbackService.Info(T("Setting {0} for part {1} has been set to {2}.", attributeName, contentPart.PartDefinition.Name, attributeValue));
            }
            else {
                _realtimeFeedbackService.Warn(T("Could not set setting {0} for part {1} because its type is not supported. Settings should be integer, boolean or string.", attributeName, contentPart.PartDefinition.Name));
            }
        }
    }
}
