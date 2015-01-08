using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using Orchard.Data;

namespace CJP.ContentSync.Services {
    public class DefaultContentRedactionService : IContentRedactionService {
        private readonly IRepository<RedactionRecord> _repository;

        public DefaultContentRedactionService(IRepository<RedactionRecord> repository) {
            _repository = repository;
        }

        public string RedactText(string text) {
            var redactions = GetRedactions();

            foreach (var redaction in redactions) {
                if (text != null) {
                    text = Regex.Replace(text, redaction.Regex, CreatePlaceholder(redaction.Placeholder));
                }
            }

            return text;
        }

        public string RestoreText(string text)
        {
            var redactions = GetRedactions();

            foreach (var redaction in redactions){
                text = text.Replace(CreatePlaceholder(redaction.Placeholder), redaction.ReplaceWith);
            }

            return text;
        }

        public RedactionOperationStatus AddRedaction(RedactionRecord redaction)
        {
            if (!PlaceholderIsValid(redaction)) 
            {
                return RedactionOperationStatus.NotUnique;
            }

            _repository.Create(redaction);

            return RedactionOperationStatus.Created;
        }

        public RedactionOperationStatus UpdateRedaction(RedactionRecord redaction)
        {
            if (!PlaceholderIsValid(redaction))
            {
                return RedactionOperationStatus.NotUnique;
            }

            _repository.Update(redaction);

            return RedactionOperationStatus.Updated;
        }

        public RedactionOperationStatus DeleteRedaction(RedactionRecord redaction)
        {
            _repository.Delete(redaction);

            return RedactionOperationStatus.Removed;
        }

        public RedactionRecord GetRedaction(int id) {
            return _repository.Get(id);
        }

        public IEnumerable<RedactionRecord> GetRedactions() {
            return _repository.Table.ToList().OrderBy(r=>r.Id);
        }

        private string CreatePlaceholder(string token) {
            return "$##" + token + "##$";
        }

        private bool PlaceholderIsValid(RedactionRecord redaction) {
            var currentRedaction = GetRedactions().FirstOrDefault(r => r.Placeholder == redaction.Placeholder);

            if (currentRedaction == null) {
                return true;
            }

            return currentRedaction.Id == redaction.Id;
        }
    }
}