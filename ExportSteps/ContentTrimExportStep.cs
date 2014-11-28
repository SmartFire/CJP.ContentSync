using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CJP.ContentSync.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Logging;

namespace CJP.ContentSync.ExportSteps
{
    public class ContentTrimExportStep : IExportEventHandler, ICustomExportStep
    {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
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
        public ContentTrimExportStep(IContentManager contentManager, IOrchardServices orchardServices) 
        {
            _contentManager = contentManager;
            _orchardServices = orchardServices;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void Exporting(ExportContext context) {
            if (!context.ExportOptions.CustomSteps.Contains("ContentTrim")) { return; }

            var xmlElement = new XElement("ContentTrim");
            var contentTypesElement = new XElement("ContentTypes");

            var settings = _orchardServices.WorkContext.CurrentSite.As<ContentSyncSettingsPart>();

            var contentTypeNames = _contentManager.GetContentTypeDefinitions().Select(ctd => ctd.Name).Except(settings.ExcludedContentTypes).ToList();

            foreach (var contentType in contentTypeNames)
            {
                contentTypesElement.Add(new XElement("add", new XAttribute("type", contentType)));
            }

            xmlElement.Add(contentTypesElement);

            var contentToKeepElement = new XElement("ContentToKeep");

            foreach (var identityPart in _contentManager.Query<IdentityPart>(contentTypeNames.ToArray()).List())
            {
                contentToKeepElement.Add(new XElement("add", new XAttribute("identifier", identityPart.Identifier)));
            }

            xmlElement.Add(contentToKeepElement);

            var rootElement = context.Document.Descendants("Orchard").FirstOrDefault();

            if (rootElement == null) 
            {
                var ex = new OrchardException(T("Could not export the content to trim because the document passed via the Export Context did not contain a node called 'Orchard'. The document was malformed."));
                Logger.Error(ex, ex.Message);
                throw ex;
            }

            rootElement.Add(xmlElement);
        }

        public void Exported(ExportContext context) {}

        public void Register(IList<string> steps) {
            steps.Add("ContentTrim");
        }
    }
}