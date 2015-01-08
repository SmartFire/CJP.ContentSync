using System.Collections.Generic;
using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using Orchard;

namespace CJP.ContentSync.Services {
    public interface ISettingRedactionService : IDependency
    {
        string GetSettingValue(string partName, string propertyName, string defaultValue);

        RedactionOperationStatus AddRedaction(SettingRedactionRecord redaction);
        RedactionOperationStatus UpdateRedaction(SettingRedactionRecord redaction);
        RedactionOperationStatus DeleteRedaction(SettingRedactionRecord redaction);
        SettingRedactionRecord GetRedaction(int id);
        IEnumerable<SettingRedactionRecord> GetRedactions();
    }
}