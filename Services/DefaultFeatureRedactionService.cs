using System.Collections.Generic;
using System.Linq;
using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using Orchard.Data;

namespace CJP.ContentSync.Services {
    public class DefaultFeatureRedactionService : IFeatureRedactionService {
        private readonly IRepository<FeatureRedactionRecord> _repository;

        public DefaultFeatureRedactionService(IRepository<FeatureRedactionRecord> repository)
        {
            _repository = repository;
        }

        public bool FeatureShouldBeEnabled(string featureId, bool defaultValue)
        {
            var redaction = GetRedactions().FirstOrDefault(r => r.FeatureId == featureId);

            return (redaction == null ? defaultValue:redaction.Enabled);
        }

        public RedactionOperationStatus AddRedaction(FeatureRedactionRecord redaction)
        {
            if (!RedactionIsValid(redaction))
            {
                return RedactionOperationStatus.NotUnique;
            }

            _repository.Create(redaction);

            return RedactionOperationStatus.Created;
        }

        public RedactionOperationStatus UpdateRedaction(FeatureRedactionRecord redaction)
        {
            if (!RedactionIsValid(redaction))
            {
                return RedactionOperationStatus.NotUnique;
            }

            _repository.Update(redaction);

            return RedactionOperationStatus.Updated;
        }

        public RedactionOperationStatus DeleteRedaction(FeatureRedactionRecord redaction)
        {
            _repository.Delete(redaction);

            return RedactionOperationStatus.Removed;
        }

        public FeatureRedactionRecord GetRedaction(int id)
        {
            return _repository.Get(id);
        }

        public IEnumerable<FeatureRedactionRecord> GetRedactions()
        {
            return _repository.Table.ToList().OrderBy(r => r.FeatureId);
        }

        private bool RedactionIsValid(FeatureRedactionRecord redaction)
        {
            var currentRedaction = GetRedactions().FirstOrDefault(r => r.FeatureId == redaction.FeatureId);

            if (currentRedaction == null)
            {
                return true;
            }

            return currentRedaction.Id == redaction.Id;
        }
    }
}