using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Dictionaries.DTO;
using JobPlatform.Core.Entities.Dictionaries;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Dictionaries.Commands.SetDictionaryItemActive;

public sealed record SetDictionaryItemActiveCommand(Guid Id, bool IsActive) : IRequest<DictionaryItemDto>
{
    public class
        SetDictionaryItemActiveCommandHandler : IRequestHandler<SetDictionaryItemActiveCommand, DictionaryItemDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public SetDictionaryItemActiveCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<DictionaryItemDto> Handle(SetDictionaryItemActiveCommand request,
            CancellationToken cancellationToken)
        {
            var item = await _db.Set<DictionaryItem>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                       ?? throw new KeyNotFoundException("Dictionary item not found.");

            var oldValue = new { item.IsActive };
            item.IsActive = request.IsActive;

            await _auditService.AddAsync(new AuditEvent(
                request.IsActive ? AuditActions.DictionaryItemActivated : AuditActions.DictionaryItemDeactivated,
                EntityType: nameof(DictionaryItem),
                EntityId: item.Id,
                OldValue: oldValue,
                NewValue: new { item.IsActive },
                UserId: _currentUser.UserId), cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new DictionaryItemDto(item.Id, item.Type, item.Code, item.Name, item.Description, item.SortOrder,
                item.IsActive);
        }
    }
}