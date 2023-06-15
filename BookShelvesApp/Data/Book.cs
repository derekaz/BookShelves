using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Data
{
    [Table("Books")]
    internal class Book : BaseTable
    {
        public string Title { get; set; }

        public Book() { }
    }
}
