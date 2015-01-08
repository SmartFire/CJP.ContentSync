namespace CJP.ContentSync.Models {
    public class FeatureRedactionRecord
    {
        public virtual int Id { get; set; }
        public virtual string FeatureId { get; set; }
        public virtual bool Enabled { get; set; }
    }
}