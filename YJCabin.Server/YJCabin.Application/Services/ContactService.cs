using System.Net.Mail;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;
using YJCabin.Application.Mapping;
using YJCabin.Domain.Entities;

namespace YJCabin.Application.Services;

public sealed class ContactService : IContactService
{
    private readonly IContactRepository _contacts;
    private readonly IUnitOfWork _unitOfWork;

    public ContactService(IContactRepository contacts, IUnitOfWork unitOfWork)
    {
        _contacts = contacts;
        _unitOfWork = unitOfWork;
    }

    public async Task SubmitAsync(CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return;
        }

        Validate(request);
        await _contacts.AddAsync(new ContactMessage
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Subject = request.Subject.Trim(),
            Body = request.Body.Trim()
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<ContactMessageDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = new ListQuery { Page = page, PageSize = pageSize };
        var (items, total) = await _contacts.ListAsync(query.SafePage, query.SafePageSize, cancellationToken);
        return new PagedResult<ContactMessageDto>
        {
            Items = items.Select(x => x.ToDto()).ToList(),
            Page = query.SafePage,
            PageSize = query.SafePageSize,
            TotalCount = total
        };
    }

    public async Task<ContactMessageDto> PatchAsync(Guid id, PatchContactMessageRequest request, CancellationToken cancellationToken = default)
    {
        var message = await _contacts.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("ContactMessage", id.ToString());
        if (request.IsRead is not null)
        {
            message.IsRead = request.IsRead.Value;
            message.ReadAt = request.IsRead.Value ? DateTimeOffset.UtcNow : null;
        }

        if (request.IsReplied is not null)
        {
            message.IsReplied = request.IsReplied.Value;
        }

        message.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return message.ToDto();
    }

    private static void Validate(CreateContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Subject) ||
            string.IsNullOrWhiteSpace(request.Body))
        {
            throw new ValidationException("body", "Name, email, subject and body are required.");
        }

        try
        {
            _ = new MailAddress(request.Email);
        }
        catch
        {
            throw new ValidationException("email", "Email is invalid.");
        }
    }
}
