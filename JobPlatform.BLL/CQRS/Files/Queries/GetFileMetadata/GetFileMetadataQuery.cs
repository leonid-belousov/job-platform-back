using JobPlatform.BLL.CQRS.Files.DTO;
using JobPlatform.Core.Entities.Files;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Files.Queries.GetFileMetadata;

public sealed record GetFileMetadataQuery(Guid FileId) : IRequest<FileDto>
{
    public class GetFileMetadataQueryHandler : IRequestHandler<GetFileMetadataQuery, FileDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public GetFileMetadataQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<FileDto> Handle(GetFileMetadataQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var file = await _db.Set<StoredFile>()
                           .FirstOrDefaultAsync(x => x.Id == request.FileId && !x.IsDeleted, cancellationToken)
                       ?? throw new KeyNotFoundException("Файл не найден.");

            if (file.UploadedByUserId != userId)
                throw new UnauthorizedAccessException("Нет доступа к файлу.");

            return new FileDto(file.Id, file.OriginalName, file.ContentType, file.SizeBytes, file.UploadedByUserId,
                file.CreatedAt);
        }
    }
}