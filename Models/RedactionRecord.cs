namespace CJP.ContentSync.Models
{
    public class RedactionRecord
    {
        public virtual int Id { get; set; }
        public virtual string Regex { get; set; }
        public virtual string Placeholder { get; set; }
        public virtual string ReplaceWith { get; set; }
    }
}