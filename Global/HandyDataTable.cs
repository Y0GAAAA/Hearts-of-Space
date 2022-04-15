using System.Data;

namespace Global.Table
{
    public class HandyDataTable : DataTable
    {
        public HandyDataTable() : base() { }
        public HandyDataTable AddColumn<T>(System.String title)
        {
            Columns.Add(title, typeof(T));
            return this;
        }
    }
}
