using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard;
using Orchard.Alias;
using Orchard.Alias.Implementation.Holder;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Logging;

namespace CJP.ContentSync.ExportSteps
{
    public class AliasesExportStep : IExportEventHandler, ICustomExportStep
    {
        private readonly IAliasHolder _aliasHolder;
        private readonly IAliasService _aliasService;
        private readonly IContentManager _contentManager;

        public AliasesExportStep(IAliasHolder aliasHolder, IAliasService aliasService, IContentManager contentManager) 
        {
            _aliasHolder = aliasHolder;
            _aliasService = aliasService;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void Exporting(ExportContext context) {
            if (!context.ExportOptions.CustomSteps.Contains("Aliases")) { return; }

            var xmlElement = new XElement("Aliases");

            var autoroutePaths = _contentManager.Query<AutoroutePart>().List().Select(p=>p.Path);
            var allAliasInfos = _aliasHolder.GetMaps().SelectMany(m=>m.GetAliases()).ToList();
            
            //we need to remove any aliases that are autoroutes because the remote conent id may not sync up with the local content id. the autoroutes will be imported as part of the content import
            var aliasInfosToExport = allAliasInfos.Where(ai => !autoroutePaths.Contains(ai.Path));

            foreach (var aliasInfo in aliasInfosToExport) 
            {
                var aliasElement = new XElement("Alias", new XAttribute("Path", aliasInfo.Path));

                var routeValuesElement = new XElement("RouteValues");
                foreach (var routeValue in aliasInfo.RouteValues) {
                    routeValuesElement.Add(new XElement("Add", new XAttribute("Key", routeValue.Key), new XAttribute("Value", routeValue.Value)));
                }

                aliasElement.Add(routeValuesElement);
                xmlElement.Add(aliasElement);
            }

            //add a collection of all the alias paths in this site so that the importing site can remove aliases that don't exist remotely
            var pathsElement = new XElement("Paths");
            foreach (var aliasInfo in allAliasInfos) {
                pathsElement.Add(new XElement("Add", new XAttribute("Path", aliasInfo.Path)));
            }

            xmlElement.Add(pathsElement);

            var rootElement = context.Document.Descendants("Orchard").FirstOrDefault();

            if (rootElement == null) 
            {
                var ex = new OrchardException(T("Could not export this site's Aliases because the document passed via the Export Context did not contain a node called 'Orchard'. The document was malformed."));
                Logger.Error(ex, ex.Message);
                throw ex;
            }

            rootElement.Add(xmlElement);
        }

        public void Exported(ExportContext context) {}

        public void Register(IList<string> steps) {
            steps.Add("Aliases");
        }
    }
}