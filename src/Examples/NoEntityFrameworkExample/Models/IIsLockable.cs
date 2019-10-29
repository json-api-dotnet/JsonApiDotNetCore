namespace NoEntityFrameworkExample.Models
{
    public interface IIsLockable
    {
        bool IsLocked { get; set; }
    }
}