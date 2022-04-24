using System.Data;

namespace Api.HOS.Json
{
    public interface IDataRow
    {
        DataRow GetRow();
    }
    public static class IDataRowExtension
    {
        public static DataTable ToDataTable(this IDataRow[] rows, DataTable table)
        {
            for (int i = 0; i < rows.Length; i++)
                table.Rows.Add(rows[i].GetRow());
            return table;
        }
    }
}
