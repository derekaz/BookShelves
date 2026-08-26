using BookShelves.Shared.Presentation.ViewModels;
using CommunityToolkit.Datasync.Server.CosmosDb;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BookShelves.WebApi.BookUserActionsDataAccess;

public class BookUserAction : CosmosTableData<BookUserAction>, IValidatableObject
{
    [Required, MinLength(1)]
    public string BookId { get; set; } = string.Empty;

    [Required, MinLength(1)]
    public string UserId { get; set; } = string.Empty;

    [Required, MinLength(1)]
    public string ActionType { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset? StartTimeUtc { get; set; }

    [Required]
    public DateTimeOffset? EndTimeUtc { get; set; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
    public BookUserActionMetadata? Details { get; set; }

    public static BookUserAction Create(string bookId, string userId, string actionType, DateTimeOffset startTimeUtc, DateTimeOffset endTimeUtc, BookUserActionMetadata details)
    {
        return new BookUserAction
        {
            BookId = bookId,
            UserId = userId,
            ActionType = actionType,
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = endTimeUtc,
            Details = details
        };
    }

    public static BookUserAction CreateToBeRead(string bookId, string userId, DateTimeOffset startTimeUtc, DateTimeOffset endTimeUtc, BookUserActionToBeReadMetadata details)
    {
        return Create(bookId, userId, BookUserActionTypes.ToBeRead, startTimeUtc, endTimeUtc, details);
    }

    public static BookUserAction CreatePagesRead(string bookId, string userId, DateTimeOffset startTimeUtc, DateTimeOffset endTimeUtc, BookUserActionPagesReadMetadata details)
    {
        return Create(bookId, userId, BookUserActionTypes.PagesRead, startTimeUtc, endTimeUtc, details);
    }

    public static BookUserAction CreateFinished(string bookId, string userId, DateTimeOffset startTimeUtc, DateTimeOffset endTimeUtc, BookUserActionFinishedMetadata details)
    {
        return Create(bookId, userId, BookUserActionTypes.Finished, startTimeUtc, endTimeUtc, details);
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!BookUserActionTypes.IsSupported(ActionType))
        {
            yield return new ValidationResult(
                $"Unsupported action type '{ActionType}'.",
                [nameof(ActionType)]);
            yield break;
        }

        if (StartTimeUtc.HasValue && EndTimeUtc.HasValue && EndTimeUtc.Value < StartTimeUtc.Value)
        {
            yield return new ValidationResult(
                "EndTimeUtc must not be earlier than StartTimeUtc.",
                [nameof(EndTimeUtc), nameof(StartTimeUtc)]);
        }

        if (Details is null)
        {
            yield return new ValidationResult(
                $"Details are required for '{ActionType}' actions.",
                [nameof(Details)]);
            yield break;
        }

        switch (ActionType)
        {
            case var action when string.Equals(action, BookUserActionTypes.ToBeRead, StringComparison.OrdinalIgnoreCase):
                if (Details is not BookUserActionToBeReadMetadata)
                {
                    yield return new ValidationResult(
                        $"'{ActionType}' actions require {nameof(BookUserActionToBeReadMetadata)} details.",
                        [nameof(Details)]);
                }
                break;

            case var action when string.Equals(action, BookUserActionTypes.PagesRead, StringComparison.OrdinalIgnoreCase):
                if (Details is not BookUserActionPagesReadMetadata pagesReadDetails)
                {
                    yield return new ValidationResult(
                        $"'{ActionType}' actions require {nameof(BookUserActionPagesReadMetadata)} details.",
                        [nameof(Details)]);
                    yield break;
                }

                if (pagesReadDetails.PagesRead < 0)
                {
                    yield return new ValidationResult(
                        "PagesRead must be non-negative.",
                        [nameof(BookUserActionPagesReadMetadata.PagesRead)]);
                }
                break;

            case var action when string.Equals(action, BookUserActionTypes.Finished, StringComparison.OrdinalIgnoreCase):
                if (Details is not BookUserActionFinishedMetadata)
                {
                    yield return new ValidationResult(
                        $"'{ActionType}' actions require {nameof(BookUserActionFinishedMetadata)} details.",
                        [nameof(Details)]);
                }
                break;
        }
    }
}
