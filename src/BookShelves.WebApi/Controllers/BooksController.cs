using BookShelves.WebApi.BooksDataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace BookShelves.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class BooksController(ILogger<BooksController> logger, BookRepository booksData) : Controller
{
    [HttpGet]
    [RequiredScopeOrAppPermission(AcceptedScope = new[] { "Books.Read" })]
    public async Task<IResult> ReadBooks()
    {
        logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(ReadBooks)}");

        var data = await booksData.GetMultipleAsync($"SELECT * FROM c WHERE c.id <> '{Book.BOOKS_UNIQUEID_RECORD_ID}'");
        return TypedResults.Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<IResult> GetBook(string id)
    {
        var data = await booksData.GetAsync(id);

        return (data != null) ? TypedResults.Ok(data) : TypedResults.NotFound();
    }

    [RequiredScopeOrAppPermission(AcceptedScope = new[] { "Books.ReadWrite" })]
    [HttpPost("new")]
    public async Task<IResult> CreateBook(Book book)
    {
        logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(CreateBook)}");

        var newUniqueId = Guid.CreateVersion7().ToString();
        var newBook = new Book()
        {
            Id = newUniqueId.ToString(),
            Title = book.Title ?? string.Empty,
            Author = book.Author ?? string.Empty,
            LastUpdateTime = book.LastUpdateTime ?? DateTime.UtcNow
        };

        await booksData.AddAsync(newBook);

        return TypedResults.Created($"/books/{book.Id}", book);
    }

    [RequiredScopeOrAppPermission(AcceptedScope = new[] { "Books.ReadWrite" })]
    [HttpPut("edit/{id}")]
    public async Task<IResult> EditBook(string id, Book inputBook)
    {
        await booksData.UpdateAsync(id, inputBook);

        var data = await booksData.GetAsync(id);

        return (data != null) ? TypedResults.Ok(data) : TypedResults.NotFound();
    }

    [RequiredScopeOrAppPermission(AcceptedScope = new[] { "Books.ReadWrite" })]
    [HttpDelete("delete/{id}")]
    public async Task<IResult> DeleteBook(string id)
    {
        logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(DeleteBook)} with id:{id}");

        try
        {
            await booksData.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Unable to delete book: {id}");
            return TypedResults.UnprocessableEntity();
        }

        return TypedResults.Ok();
    }
}
