﻿using Orchard;

namespace CJP.ContentSync.Services
{
    public interface IContentExportService : IDependency {
        string GetContentExportText();
    }
}