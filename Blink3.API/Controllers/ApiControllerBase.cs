using System.Net.Mime;
using Blink3.Core.DiscordAuth.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers;

/// <summary>
///     Base class for API controllers.
/// </summary>
/// <remarks>
///     This class provides common functionality and properties that can be used by derived API controllers.
/// </remarks>
[Consumes(MediaTypeNames.Application.Json)]
[ProducesErrorResponseType(typeof(ProblemDetails))]
[SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(ProblemDetails))]
[Route("api/[controller]")]
[ApiController]
[Authorize]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    ///     Represents an Unauthorized Access message.
    /// </summary>
    private const string UnauthorizedAccessMessage = "You are not authorised to view this item.";

    /// <summary>
    ///     The message displayed when an item could not be found.
    /// </summary>
    private const string NotFoundAccessMessage = "Could not find an item with that ID";

    /// <summary>
    ///     Represents the user ID of the logged-in user.
    /// </summary>
    protected ulong UserId => User.GetUserId();

    /// <summary>
    ///     Creates a ProblemDetails object with a 404 Not Found status code and a specific error message.
    /// </summary>
    /// <returns>
    ///     Returns an ObjectResult that represents a ProblemDetails object with a 404 Not Found status code and a
    ///     specific error message.
    /// </returns>
    private ObjectResult ProblemForMissingItem()
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            detail: NotFoundAccessMessage
        );
    }

    /// <summary>
    ///     Generates a problem response for unauthorized access.
    /// </summary>
    /// <returns>
    ///     An <see cref="ObjectResult" /> representing a problem response with the following attributes:
    ///     - StatusCode: Status code indicating unauthorized access (401 Unauthorized).
    ///     - Detail: A message indicating that the user is not authorized to view the item.
    /// </returns>
    private ObjectResult ProblemForUnauthorizedAccess()
    {
        return Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            detail: UnauthorizedAccessMessage
        );
    }

    /// <summary>
    ///     Checks access to a resource based on the provided user ID.
    /// </summary>
    /// <param name="userId">The user ID to check access against.</param>
    /// <returns>
    ///     An <see cref="ObjectResult" /> representing a problem response if any.
    ///     Returns <c>null</c> if there are no errors.
    /// </returns>
    protected ObjectResult? CheckAccess(ulong? userId)
    {
        if (userId is null) return ProblemForMissingItem();
        return userId != UserId ? ProblemForUnauthorizedAccess() : null;
    }
}