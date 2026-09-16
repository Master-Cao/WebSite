using YJCabin.Domain.Common;

namespace YJCabin.Domain.Entities;

public class ContactMessage : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Website { get; set; }
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
