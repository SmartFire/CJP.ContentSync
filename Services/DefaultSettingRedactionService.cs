using System.Collections.Generic;
using System.Linq;
using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using Orchard.Data;

namespace CJP.ContentSync.Services {
    public class DefaultSettingRedactionService : ISettingRedactionService 
    {
        private readonly IRepository<SettingRedactionRecord> _repository;

        public DefaultSettingRedactionService(IRepository<SettingRedactionRecord> repository) {
            _repository = repository;
        }

        public string GetSettingValue(string partName, string propertyName, string defaultValue)
        {
            var redaction = GetRedactions().FirstOrDefault(r => r.SettingName == string.Format("{0}.{1}", partName, propertyName));

            return (redaction == null ? defaultValue : redaction.Value);
        }

        public RedactionOperationStatus AddRedaction(SettingRedactionRecord redaction)
        {
            if (!RedactionIsValid(redaction))
            {
                return RedactionOperationStatus.NotUnique;
            }

            _repository.Create(redaction);

            return RedactionOperationStatus.Created;
        }

        public RedactionOperationStatus UpdateRedaction(SettingRedactionRecord redaction)
        {
            if (!RedactionIsValid(redaction))
            {
                return RedactionOperationStatus.NotUnique;
            }

            _repository.Update(redaction);

            return RedactionOperationStatus.Updated;
        }

        public RedactionOperationStatus DeleteRedaction(SettingRedactionRecord redaction)
        {
            _repository.Delete(redaction);

            return RedactionOperationStatus.Removed;
        }

        public SettingRedactionRecord GetRedaction(int id)
        {
            return _repository.Get(id);
        }

        public IEnumerable<SettingRedactionRecord> GetRedactions()
        {
            return _repository.Table.ToList().OrderBy(r => r.SettingName);
        }

        private bool RedactionIsValid(SettingRedactionRecord redaction)
        {
            var currentRedaction = GetRedactions().FirstOrDefault(r => r.SettingName == redaction.SettingName);

            if (currentRedaction == null)
            {
                return true;
            }

            return currentRedaction.Id == redaction.Id;
        }
    }
}