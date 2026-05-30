using System.Globalization;
using System.Reflection;
using System.Text;
using JobPlatform.BLL.Common.Interfaces;

namespace JobPlatform.BLL.Common.Export;

public sealed class SimpleCsvExportService : ICsvExportService
{
    public byte[] Export<T>(IReadOnlyCollection<T> rows)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var builder = new StringBuilder();

        builder.AppendLine(string.Join(",", properties.Select(x => Escape(x.Name))));

        foreach (var row in rows)
        {
            var values = properties.Select(property =>
            {
                var value = property.GetValue(row);
                return Escape(FormatValue(value));
            });
            builder.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            DateOnly dateOnly => dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string Escape(string value)
    {
        var normalized = value.Replace("\r", " ").Replace("\n", " ");
        if (normalized.Contains(',') || normalized.Contains('"'))
        {
            normalized = "\"" + normalized.Replace("\"", "\"\"") + "\"";
        }

        return normalized;
    }
}