using System.Collections.Generic;
using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using Orchard;

namespace CJP.ContentSync.Services
{
    public interface IContentRedactionService : IDependency
    {
        string RedactText(string text);
        string RestoreText(string text);

        RedactionOperationStatus AddRedaction(RedactionRecord redaction);
        RedactionOperationStatus UpdateRedaction(RedactionRecord redaction);
        RedactionOperationStatus DeleteRedaction(RedactionRecord redaction);
        RedactionRecord GetRedaction(int id);
        IEnumerable<RedactionRecord> GetRedactions();
    }
}