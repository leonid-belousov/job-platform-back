using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Dictionaries.DTO;
using JobPlatform.Core.Entities.Dictionaries;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Dictionaries.Commands.CreateDictionaryItem;

public sealed record CreateDictionaryItemCommand(
    string Type,
    string Code,
    string Name,
    string? NameEn,
    string? Description,
    string? DescriptionEn,
    int SortOrder,
    bool IsActive = true) : IRequest<DictionaryItemDto>
{
    public class CreateDictionaryItemCommandHandler : IRequestHandler<CreateDictionaryItemCommand, DictionaryItemDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public CreateDictionaryItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<DictionaryItemDto> Handle(CreateDictionaryItemCommand request,
            CancellationToken cancellationToken)
        {
            var code = request.Code.Trim().ToLowerInvariant();
            var exists = await _db.Set<DictionaryItem>()
                .AnyAsync(x => x.Type == request.Type && x.Code == code, cancellationToken);
            if (exists)
                throw new InvalidOperationException("Dictionary item with the same type and code already exists.");

            var item = new DictionaryItem
            {
                Type = request.Type,
                Code = code,
                Name = request.Name.Trim(),
                NameEn = string.IsNullOrWhiteSpace(request.NameEn) ? null : request.NameEn.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                DescriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim(),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive
            };

            await _db.Set<DictionaryItem>().AddAsync(item, cancellationToken);
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.DictionaryItemCreated,
                EntityType: nameof(DictionaryItem),
                EntityId: item.Id,
                NewValue: new { item.Type, item.Code, item.Name, item.NameEn, item.SortOrder, item.IsActive },
                UserId: _currentUser.UserId), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new DictionaryItemDto(item.Id, item.Type, item.Code, item.Name, item.NameEn, item.Description,
                item.DescriptionEn, item.SortOrder, item.IsActive);
        }
    }
}