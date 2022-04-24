using System.Data;

namespace Global.Util
{
    public static class DataTableExtension
    {
        public static DataRow NewRow(this DataTable table, params object[] obj)
        {
            var row = table.NewRow();
            row.ItemArray = obj;
            return row;
        }
    }
}
