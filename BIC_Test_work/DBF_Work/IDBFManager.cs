using System.Data;

namespace DBF_Work
{
    public interface IDBFManager
    {
        DataTable Execute(string command);
    }
}
