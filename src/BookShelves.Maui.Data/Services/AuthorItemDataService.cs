using BookShelves.Maui.Data.SyncTest;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BookShelves.Maui.Data.Services;

public class AuthorItemDataService(IServiceProvider serviceProvider) : IAuthorItemDataService
{
    //private readonly Expression<Func<AuthorItem, bool>> changedAuthors = p =>
    //    p.UpdateType == "C" || p.UpdateType == "U" || p.UpdateType == "D";

    public async Task<bool> CreateAuthorAsync(AuthorItemViewModel author)
    {
        var newAuthor = AuthorItem.FromAuthorItemViewModel(author);
        //newAuthor.Revision = author.Revision + 1;
        //newAuthor.UpdateType = "C";
        newAuthor.UpdatedAt = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<AuthorItem>();
        await repo.AddAsync(newAuthor);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> CreateAuthorFromSyncAsync(AuthorItem author)
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<AuthorItem>();
        await repo.AddAsync(author);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAuthorAsync(AuthorItemViewModel author)
    {
        var localAuthor = AuthorItem.FromAuthorItemViewModel(author);
        //localAuthor.Revision = author.Revision + 1;
        //localAuthor.UpdateType = "C";
        localAuthor.UpdatedAt = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<AuthorItem>();
        await repo.UpdateAsync(localAuthor);

        var result = await uow.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> UpdateAuthorFromSyncAsync(AuthorItem newAuthor)
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<AuthorItem>();
        await repo.AddAsync(newAuthor);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAuthorAsync(AuthorItemViewModel author, bool softDelete = false)
    {
        var localAuthor = AuthorItem.FromAuthorItemViewModel(author);

        if (softDelete)
        {
            //localAuthor.Revision = author.Revision + 1;
            //localAuthor.UpdateType = "D";
            localAuthor.UpdatedAt = DateTime.UtcNow;

            await using var uow1 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
            var repo1 = uow1.GetRepository<AuthorItem>();
            await repo1.UpdateAsync(localAuthor);
            return await uow1.SaveChangesAsync() > 0;
        }

        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo2 = uow2.GetRepository<AuthorItem>();
        await repo2.DeleteAsync(localAuthor);
        return await uow2.SaveChangesAsync() > 0;
    }

    //public async Task<IEnumerable<LocalAuthor>> GetAllEntitiesAsync()
    //{
    //    return await _unitOfWork.LocalBooks.GetAllAsync();
    //}

    public async Task<IEnumerable<AuthorItemViewModel>> GetAuthorsAsync(bool includeSoftDeleted = false)
    {
        if (includeSoftDeleted)
        {
            await using var uow1 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
            var repo1 = uow1.GetRepository<AuthorItem>();
            return (await repo1.GetAllAsync()).Select(b => b.ToAuthorItemViewModel());
        }

        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo2 = uow2.GetRepository<AuthorItem>();

        // var localAuthors = await repo2.FindAsync(b => b.UpdateType != "D");
        var localAuthors = await repo2.GetAllAsync();
        return localAuthors.Select(b => b.ToAuthorItemViewModel());
    }

    //public async Task<AuthorItem?> GetAuthorWithServerIdAsync(int serverId)
    //{
    //    await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
    //    var repo = uow.GetRepository<AuthorItem>();
    //    return await repo.FindAsync(b => b.ServerId == serverId).ContinueWith(t => t.Result.FirstOrDefault());
    //}

    //public async Task<IEnumerable<AuthorItem>> GetChangedAuthorsAsync()
    //{
    //    await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
    //    var repo = uow.GetRepository<AuthorItem>();
    //    return await repo.FindAsync(changedAuthors);
    //}

    public async Task ServerSyncAsync()
    {
        await using var uow1 = serviceProvider.GetRequiredService<SyncDbContext>();
        await uow1.SynchronizeAsync();
    }
}
