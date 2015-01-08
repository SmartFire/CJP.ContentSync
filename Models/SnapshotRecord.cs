using System;
using Orchard.Data.Conventions;

namespace CJP.ContentSync.Models {
    public class SnapshotRecord
    {
        public virtual int Id { get; set; }
        public virtual DateTime TimeTaken { get; set; }
        [StringLengthMax]
        public virtual string Data { get; set; }
    }
}