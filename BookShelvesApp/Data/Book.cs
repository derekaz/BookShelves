﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Data
{
    [Table(Constants.BookTable)]
    public class Book : BaseTable, IBook
    {
        public new string Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public Book() { }
    }
}
