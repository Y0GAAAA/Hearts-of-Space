using System.Data;

namespace Api.HOS.Json
{
    public interface IDataRow
    {
        object[] GetRow();
    }
    public static class IDataRowExtension
    {
        public static DataTable ToDataTable(this IDataRow[] rows, DataTable table)
        {
            if (rows == null || rows.Length == 0)
                return table;
            for (int i = 0; i < rows.Length; i++)
                table.Rows.Add(rows[i].GetRow());
            return table;
        }
    }
}
