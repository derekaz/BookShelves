using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Data
{
    public class BaseTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
