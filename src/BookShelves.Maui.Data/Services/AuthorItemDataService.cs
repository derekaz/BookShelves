using BookShelves.Maui.Data.SyncTest;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BookShelves.Maui.Data.Services;

public class AuthorItemDataService(IServiceProvider serviceProvider) : IAuthorItemDataService
{
    public async Task<bool> CreateAuthorAsync(AuthorItemViewModel author)
    {
        var newAuthor = Author.FromAuthorItemViewModel(author);
        if (string.IsNullOrEmpty(newAuthor.Id))
        {
            newAuthor.Id = Guid.CreateVersion7().ToString();
        }

        newAuthor.UpdatedAt = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<Author>();
        await repo.AddAsync(newAuthor);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> CreateAuthorFromSyncAsync(Author author)
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<Author>();
        await repo.AddAsync(author);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAuthorAsync(AuthorItemViewModel author)
    {
        var localAuthor = Author.FromAuthorItemViewModel(author);
        localAuthor.UpdatedAt = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<Author>();
        await repo.UpdateAsync(localAuthor);

        var result = await uow.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> UpdateAuthorFromSyncAsync(Author newAuthor)
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<Author>();
        await repo.AddAsync(newAuthor);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAuthorAsync(AuthorItemViewModel author, bool softDelete = false)
    {
        var localAuthor = Author.FromAuthorItemViewModel(author);

        if (softDelete)
        {
            localAuthor.UpdatedAt = DateTime.UtcNow;

            await using var uow1 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
            var repo1 = uow1.GetRepository<Author>();
            await repo1.UpdateAsync(localAuthor);
            return await uow1.SaveChangesAsync() > 0;
        }

        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo2 = uow2.GetRepository<Author>();
        await repo2.DeleteAsync(localAuthor);
        return await uow2.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<AuthorItemViewModel>> GetAuthorsAsync(bool includeSoftDeleted = false)
    {
        if (includeSoftDeleted)
        {
            await using var uow1 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
            var repo1 = uow1.GetRepository<Author>();
            return (await repo1.GetAllAsync()).Select(b => b.ToAuthorItemViewModel());
        }

        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo2 = uow2.GetRepository<Author>();

        var localAuthors = await repo2.GetAllAsync();
        return localAuthors.Select(b => b.ToAuthorItemViewModel());
    }

    public async Task ServerSyncAsync()
    {
        await using var uow = serviceProvider.GetRequiredService<ISyncUnitOfWork<SyncDbContext>>();
        await uow.SynchronizeAsync();
    }
}
