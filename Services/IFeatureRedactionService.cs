using System.Collections.Generic;
using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using Orchard;

namespace CJP.ContentSync.Services {
    public interface IFeatureRedactionService : IDependency
    {
        bool FeatureShouldBeEnabled(string featureId, bool defaultValue);

        RedactionOperationStatus AddRedaction(FeatureRedactionRecord redaction);
        RedactionOperationStatus UpdateRedaction(FeatureRedactionRecord redaction);
        RedactionOperationStatus DeleteRedaction(FeatureRedactionRecord redaction);
        FeatureRedactionRecord GetRedaction(int id);
        IEnumerable<FeatureRedactionRecord> GetRedactions();
    }
}