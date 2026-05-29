using JobPlatform.BLL.CQRS.Files.DTO;
using JobPlatform.Core.Entities.Files;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Files.Queries.GetFileDownload;

public sealed record GetFileDownloadQuery(Guid FileId) : IRequest<FileDownloadDto>
{
    public class GetFileDownloadQueryHandler : IRequestHandler<GetFileDownloadQuery, FileDownloadDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorage;

        public GetFileDownloadQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IFileStorageService fileStorage)
        {
            _db = db;
            _currentUser = currentUser;
            _fileStorage = fileStorage;
        }

        public async Task<FileDownloadDto> Handle(GetFileDownloadQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            
            var file = await _db.Set<StoredFile>()
                           .FirstOrDefaultAsync(x => x.Id == request.FileId && !x.IsDeleted, cancellationToken)
                       ?? throw new KeyNotFoundException("Файл не найден.");

            if (file.UploadedByUserId != userId)
                throw new UnauthorizedAccessException("Нет доступа к файлу.");

            var stream = await _fileStorage.OpenReadAsync(file.StorageKey, cancellationToken);
            
            return new FileDownloadDto(stream, file.OriginalName, file.ContentType);
        }
    }
}