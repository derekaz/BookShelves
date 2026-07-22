using BookShelves.Maui.Data.SyncTest;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BookShelves.Maui.Data.Services;

public class MauiAuthorDataService(IServiceProvider serviceProvider) : IAuthorDataService
{
    public async Task<bool> CreateAuthorAsync(AuthorViewModel author)
    {
        var newAuthor = Author.FromAuthorViewModel(author, true);
        newAuthor.UpdatedAt = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<Author>();
        await repo.AddAsync(newAuthor);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<AuthorViewModel>> GetAuthorsAsync(bool includeSoftDeleted = false)
    {
        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo2 = uow2.GetRepository<Author>();
        var localAuthors = await repo2.GetAllAsync();
        return localAuthors.Select(b => b.ToAuthorViewModel());
    }

    public async Task<bool> UpdateAuthorAsync(AuthorViewModel author)
    {
        var localAuthor = Author.FromAuthorViewModel(author, false);
        localAuthor.UpdatedAt = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<Author>();
        await repo.UpdateAsync(localAuthor);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAuthorAsync(AuthorViewModel author)
    {
        var localAuthor = Author.FromAuthorViewModel(author, false);

        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo2 = uow2.GetRepository<Author>();
        await repo2.DeleteAsync(localAuthor);
        return await uow2.SaveChangesAsync() > 0;
    }
}
