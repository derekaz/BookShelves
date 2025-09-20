using BookShelves.Shared.DataInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Maui.Data.Services;

public class LocalBookFactory : IBookFactory
{
    public IBook Create() => new Models.LocalBook();
}
