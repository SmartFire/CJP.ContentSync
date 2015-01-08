using System.Collections.Generic;
using System.Web.Routing;
using CJP.ContentSync.Services;
using JetBrains.Annotations;
using Orchard;
using Orchard.Core.Common.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Environment.Extensions;
using Orchard.Services;

namespace CJP.ContentSync.Drivers {
    [OrchardSuppressDependency("Orchard.Core.Common.Drivers.BodyPartDriver")]
    [UsedImplicitly]
    public class RedactedBodyPartDriver : BodyPartDriver {
        private readonly IContentRedactionService _textRedactionService;

        public RedactedBodyPartDriver(IOrchardServices services, IEnumerable<IHtmlFilter> htmlFilters, RequestContext requestContext, IContentRedactionService textRedactionService)
            : base(services, htmlFilters, requestContext) {
            _textRedactionService = textRedactionService;
        }


        protected override void Importing(BodyPart part, Orchard.ContentManagement.Handlers.ImportContentContext context) {
            var importedText = context.Attribute(part.PartDefinition.Name, "Text");
            if (importedText != null) {
                part.Text = _textRedactionService.RestoreText(importedText);
            }
        }

        protected override void Exporting(BodyPart part, Orchard.ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Text", _textRedactionService.RedactText(part.Text));
        }
    }
}