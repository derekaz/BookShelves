using BookShelves.Shared.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Maui.Data.Services;

public class LocalBookFactory : IBookFactory
{
    public IBook CreateBook() => new Models.LocalBook();
}
