using BookShelves.Maui.Data.Models;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace BookShelves.Maui.Data.Services;

public class AuthorItemDataService(IServiceProvider serviceProvider) : IAuthorItemDataService
{
    private readonly Expression<Func<LocalAuthor, bool>> changedAuthors = p =>
        p.UpdateType == "C" || p.UpdateType == "U" || p.UpdateType == "D";

    public async Task<bool> CreateAuthorAsync(AuthorItemViewModel author)
    {
        var newAuthor = LocalAuthor.FromAuthorItemViewModel(author);
        newAuthor.Revision = author.Revision + 1;
        newAuthor.UpdateType = "C";
        newAuthor.LastUpdateTime = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalAuthor>();
        await repo.AddAsync(newAuthor);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> CreateAuthorFromSyncAsync(LocalAuthor author)
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalAuthor>();
        await repo.AddAsync(author);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAuthorAsync(AuthorItemViewModel author)
    {
        var localAuthor = LocalAuthor.FromAuthorItemViewModel(author);
        localAuthor.Revision = author.Revision + 1;
        localAuthor.UpdateType = "C";
        localAuthor.LastUpdateTime = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalAuthor>();
        await repo.UpdateAsync(localAuthor);

        var result = await uow.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> UpdateAuthorFromSyncAsync(LocalAuthor newAuthor)
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalAuthor>();
        await repo.AddAsync(newAuthor);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAuthorAsync(AuthorItemViewModel author, bool softDelete = false)
    {
        var localAuthor = LocalAuthor.FromAuthorItemViewModel(author);

        if (softDelete)
        {
            localAuthor.Revision = author.Revision + 1;
            localAuthor.UpdateType = "D";
            localAuthor.LastUpdateTime = DateTime.UtcNow;

            await using var uow1 = serviceProvider.GetRequiredService<IUnitOfWork>();
            var repo1 = uow1.GetRepository<LocalAuthor>();
            await repo1.UpdateAsync(localAuthor);
            return await uow1.SaveChangesAsync() > 0;
        }

        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo2 = uow2.GetRepository<LocalAuthor>();
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
            await using var uow1 = serviceProvider.GetRequiredService<IUnitOfWork>();
            var repo1 = uow1.GetRepository<LocalAuthor>();
            return (await repo1.GetAllAsync()).Select(b => b.ToAuthorItemViewModel());
        }

        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo2 = uow2.GetRepository<LocalAuthor>();

        var localAuthors = await repo2.FindAsync(b => b.UpdateType != "D");

        return localAuthors.Select(b => b.ToAuthorItemViewModel());
    }

    public async Task<LocalAuthor?> GetAuthorWithServerIdAsync(int serverId)
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalAuthor>();
        return await repo.FindAsync(b => b.ServerId == serverId).ContinueWith(t => t.Result.FirstOrDefault());
    }

    public async Task<IEnumerable<LocalAuthor>> GetChangedAuthorsAsync()
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalAuthor>();
        return await repo.FindAsync(changedAuthors);
    }
}
