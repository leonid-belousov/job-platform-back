using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Dictionaries.DTO;
using JobPlatform.Core.Entities.Dictionaries;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Dictionaries.Commands.UpdateDictionaryItem;

public sealed record UpdateDictionaryItemCommand(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive) : IRequest<DictionaryItemDto>
{
    public class UpdateDictionaryItemCommandHandler : IRequestHandler<UpdateDictionaryItemCommand, DictionaryItemDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public UpdateDictionaryItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<DictionaryItemDto> Handle(UpdateDictionaryItemCommand request,
            CancellationToken cancellationToken)
        {
            var item = await _db.Set<DictionaryItem>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                       ?? throw new KeyNotFoundException("Dictionary item not found.");

            var code = request.Code.Trim().ToLowerInvariant();
            var duplicateExists = await _db.Set<DictionaryItem>()
                .AnyAsync(x => x.Id != request.Id && x.Type == item.Type && x.Code == code, cancellationToken);
            if (duplicateExists)
                throw new InvalidOperationException("Dictionary item with the same type and code already exists.");

            var oldValue = new { item.Type, item.Code, item.Name, item.Description, item.SortOrder, item.IsActive };

            item.Code = code;
            item.Name = request.Name.Trim();
            item.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            item.SortOrder = request.SortOrder;
            item.IsActive = request.IsActive;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.DictionaryItemUpdated,
                EntityType: nameof(DictionaryItem),
                EntityId: item.Id,
                OldValue: oldValue,
                NewValue: new { item.Type, item.Code, item.Name, item.Description, item.SortOrder, item.IsActive },
                UserId: _currentUser.UserId), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new DictionaryItemDto(item.Id, item.Type, item.Code, item.Name, item.Description, item.SortOrder,
                item.IsActive);
        }
    }
}