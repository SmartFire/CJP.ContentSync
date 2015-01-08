using System;
using System.Collections.Generic;
using System.Linq;
using CJP.ContentSync.Models;
using Orchard.Data;
using Orchard.Services;

namespace CJP.ContentSync.Services {
    public class DefaultSnapshotService : ISnapshotService 
    {
        private readonly IRepository<SnapshotRecord> _snapshotRepository;
        private readonly IContentExportService _contentExportService;
        private readonly IClock _clock;

        public DefaultSnapshotService(IRepository<SnapshotRecord> snapshotRepository, IContentExportService contentExportService, IClock clock)
        {
            _snapshotRepository = snapshotRepository;
            _contentExportService = contentExportService;
            _clock = clock;
        }

        public SnapshotRecord GetLatestSnaphot() 
        {
            return _snapshotRepository.Table.OrderByDescending(ss => ss.TimeTaken).FirstOrDefault();
        }

        public SnapshotRecord GetSnaphotAt(DateTime time)
        {
            return _snapshotRepository.Table.Where(ss=>ss.TimeTaken<= time).OrderByDescending(ss => ss.TimeTaken).FirstOrDefault();
        }

        public SnapshotRecord GetSnaphot(int id) {
            return _snapshotRepository.Get(id);
        }

        public IEnumerable<SnapshotRecord> GetSnaphots() {
            return _snapshotRepository.Table.ToList();
        }

        public void TakeSnaphot() {
            _snapshotRepository.Create(new SnapshotRecord {
                TimeTaken = _clock.UtcNow,
                Data = _contentExportService.GetContentExportText()
            });
        }
    }
}