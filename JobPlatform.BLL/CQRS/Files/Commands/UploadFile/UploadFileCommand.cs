using JobPlatform.BLL.CQRS.Files.DTO;
using JobPlatform.Core.Entities.Files;
using JobPlatform.DAL.Interfaces;
using MediatR;

namespace JobPlatform.BLL.CQRS.Files.Commands.UploadFile;

public sealed record UploadFileCommand(
    Stream Content,
    string OriginalName,
    string ContentType,
    long SizeBytes) : IRequest<FileDto>
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, FileDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorage;

        public UploadFileCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IFileStorageService fileStorage)
        {
            _db = db;
            _currentUser = currentUser;
            _fileStorage = fileStorage;
        }

        public async Task<FileDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var storageKey = await _fileStorage.SaveAsync(
                request.Content,
                request.OriginalName,
                request.ContentType,
                request.SizeBytes,
                cancellationToken);

            var file = new StoredFile
            {
                OriginalName = Path.GetFileName(request.OriginalName),
                StorageKey = storageKey,
                ContentType = request.ContentType,
                SizeBytes = request.SizeBytes,
                UploadedByUserId = userId
            };

            await _db.Set<StoredFile>().AddAsync(file, cancellationToken);
            
            await _db.SaveChangesAsync(cancellationToken);

            return new FileDto(file.Id, file.OriginalName, file.ContentType, file.SizeBytes, file.UploadedByUserId,
                file.CreatedAt);
        }
    }
}