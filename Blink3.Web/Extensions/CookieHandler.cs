using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Blink3.Web.Extensions;

/// <inheritdoc />
public class CookieHandler : DelegatingHandler
{
    /// <summary>
    ///     Sends an HTTP request with credentials as an asynchronous operation.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains the HTTP response message.
    /// </returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        return await base.SendAsync(request, cancellationToken);
    }
}