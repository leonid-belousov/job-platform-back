using System.IO.Compression;
using System.Net;
using System.Text;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Applications.Queries.ExportVacancyApplicationsXlsx;

public sealed record ExportVacancyApplicationsXlsxQuery(Guid VacancyId) : IRequest<byte[]>
{
    public sealed class Handler : IRequestHandler<ExportVacancyApplicationsXlsxQuery, byte[]>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<byte[]> Handle(ExportVacancyApplicationsXlsxQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var vacancy = await _db.Set<JobVacancy>()
                              .AsNoTracking()
                              .FirstOrDefaultAsync(x => x.Id == request.VacancyId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Vacancy not found.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == vacancy.CompanyId && x.UserId == userId && x.Status == "Active" && !x.IsDeleted,
                    cancellationToken);

            if (!hasAccess)
            {
                throw new UnauthorizedAccessException();
            }

            var rows = await _db.Set<JobApplication>()
                .AsNoTracking()
                .Include(x => x.CandidateProfile)
                .Where(x => x.VacancyId == request.VacancyId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new object?[]
                {
                    x.Id,
                    x.Status,
                    (x.CandidateProfile.FirstName + " " + x.CandidateProfile.LastName).Trim(),
                    x.CandidateProfile.DesiredPosition,
                    x.CandidateProfile.CountryOfResidence,
                    x.CandidateProfile.City,
                    x.CoverLetter,
                    x.CreatedAt
                })
                .ToArrayAsync(cancellationToken);

            var table = new List<object?[]>
            {
                new object?[]
                {
                    "Id",
                    "Status",
                    "CandidateName",
                    "DesiredPosition",
                    "CountryOfResidence",
                    "City",
                    "CoverLetter",
                    "CreatedAt"
                }
            };
            table.AddRange(rows);

            return BuildXlsx(table);
        }

        private static byte[] BuildXlsx(IReadOnlyList<object?[]> rows)
        {
            using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                AddEntry(archive, "[Content_Types].xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                      <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                      <Default Extension="xml" ContentType="application/xml"/>
                      <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                      <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                      <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                    </Types>
                    """);

                AddEntry(archive, "_rels/.rels", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                      <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                    </Relationships>
                    """);

                AddEntry(archive, "xl/_rels/workbook.xml.rels", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                      <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                      <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                    </Relationships>
                    """);

                AddEntry(archive, "xl/workbook.xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                      <sheets>
                        <sheet name="Applications" sheetId="1" r:id="rId1"/>
                      </sheets>
                    </workbook>
                    """);

                AddEntry(archive, "xl/styles.xml", """
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                      <fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><name val="Calibri"/></font></fonts>
                      <fills count="1"><fill><patternFill patternType="none"/></fill></fills>
                      <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
                      <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
                      <cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/></cellXfs>
                    </styleSheet>
                    """);

                AddEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(rows));
            }

            return stream.ToArray();
        }

        private static string BuildWorksheetXml(IReadOnlyList<object?[]> rows)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
            builder.Append("<sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"1\" topLeftCell=\"A2\" activePane=\"bottomLeft\" state=\"frozen\"/></sheetView></sheetViews>");
            builder.Append("<cols><col min=\"1\" max=\"1\" width=\"38\" customWidth=\"1\"/><col min=\"2\" max=\"8\" width=\"24\" customWidth=\"1\"/></cols>");
            builder.Append("<sheetData>");

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var rowNumber = rowIndex + 1;
                builder.Append($"<row r=\"{rowNumber}\">");
                var row = rows[rowIndex];
                for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
                {
                    var reference = GetCellReference(columnIndex, rowNumber);
                    var style = rowIndex == 0 ? " s=\"1\"" : string.Empty;
                    builder.Append($"<c r=\"{reference}\" t=\"inlineStr\"{style}><is><t>{ToCellText(row[columnIndex])}</t></is></c>");
                }

                builder.Append("</row>");
            }

            builder.Append("</sheetData>");
            builder.Append("<autoFilter ref=\"A1:H1\"/>");
            builder.Append("</worksheet>");
            return builder.ToString();
        }

        private static void AddEntry(ZipArchive archive, string path, string content)
        {
            var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream, new UTF8Encoding(false));
            writer.Write(content.Trim());
        }

        private static string GetCellReference(int zeroBasedColumnIndex, int rowNumber)
        {
            var dividend = zeroBasedColumnIndex + 1;
            var columnName = string.Empty;
            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName + rowNumber;
        }

        private static string ToCellText(object? value)
            => WebUtility.HtmlEncode(value switch
            {
                null => string.Empty,
                DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O"),
                DateTime dateTime => dateTime.ToString("O"),
                _ => value.ToString() ?? string.Empty
            });
    }
}
