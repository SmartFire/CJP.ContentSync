namespace CJP.ContentSync.Models
{
    public class SelectableItem<T>
    {
        public T Item { get; set; }
        public bool IsSelected { get; set; }
    }
}