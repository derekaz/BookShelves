using BookShelves.Maui.Data.Models;
using BookShelves.Shared.DataInterfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Maui.Data.Services;

public class BookShelvesContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    public string DbPath { get; private set; }

    public BookShelvesContext(string dbPath)
    {
        DbPath = dbPath;
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
        SQLitePCL.Batteries_V2.Init();
        base.OnConfiguring(optionsBuilder);
    }
}
