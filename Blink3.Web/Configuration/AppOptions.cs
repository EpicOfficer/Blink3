// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Blink3.Web.Configuration;

/// <summary>
///     Represents the options for the application.
/// </summary>
/// <remarks>
///     The <c>AppOptions</c> class provides properties to configure the application.
/// </remarks>
public class AppOptions
{
    /// <summary>
    ///     Represents the address of the API.
    /// </summary>
    public string ApiAddress { get; set; } = default!;

    /// <summary>
    ///     Represents the Discord Client ID for the application.
    /// </summary>
    public string ClientId { get; set; } = default!;
}