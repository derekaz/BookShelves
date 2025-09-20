using BookShelves.Shared.DataInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.WebShared.Data;

public class BookFactory : IBookFactory
{
    public IBook Create() => new Book();
}
