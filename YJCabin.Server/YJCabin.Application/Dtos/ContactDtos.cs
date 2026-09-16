namespace YJCabin.Application.Dtos;

public sealed class CreateContactRequest
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public string? Website { get; init; }
}

public sealed class ContactMessageDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsRead { get; init; }
    public bool IsReplied { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
}

public sealed class PatchContactMessageRequest
{
    public bool? IsRead { get; init; }
    public bool? IsReplied { get; init; }
}
