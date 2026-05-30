namespace JobPlatform.BLL.Common.Interfaces;

public interface ICsvExportService
{
    byte[] Export<T>(IReadOnlyCollection<T> rows);
}