using System;
using System.Collections.Generic;
using CJP.ContentSync.Models;
using Orchard;

namespace CJP.ContentSync.Services
{
    public interface ISnapshotService : IDependency
    {
        SnapshotRecord GetLatestSnaphot();
        SnapshotRecord GetSnaphotAt(DateTime time);
        SnapshotRecord GetSnaphot(int id);
        IEnumerable<SnapshotRecord> GetSnaphots();
        void TakeSnaphot();
    }
}