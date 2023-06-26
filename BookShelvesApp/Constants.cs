using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves
{
    internal static class Constants
    {
        public const string LocalDbFile = "bookshelves.db";
        public const string BookTable = "books";

        public const string CreateBookTableStatement = $"CREATE TABLE IF NOT EXISTS {BookTable} (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title VARCHAR(255), Author VARCHAR(255));";

        public const string AllBooksQuery = $"SELECT * FROM {BookTable}";
        //public const string EmployeesAndDepartmentQuery = $"SELECT e.*, d.Name as DepartmentName FROM {EmployeeTable} e JOIN {DepartmentTable} d ON e.DepartmentId = d.Id";

        //public const string DepartmentDetailsRoute = "DepartmentDetailsPage";
        //public const string EmployeeDetailsRoute = "EmployeeDetailsPage";
    }
}