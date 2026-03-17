using CsvHelper.Configuration;

namespace ExpenseAnalyzer.Application.DTOs;

public sealed class ImportCsvRowDtoMap : ClassMap<ImportCsvRowDto>
{
    public ImportCsvRowDtoMap()
    {
        Map(m => m.Date).Name("Date");
        Map(m => m.Description).Name("Description");
        Map(m=> m.Amount).Name("Amount");
    }
}