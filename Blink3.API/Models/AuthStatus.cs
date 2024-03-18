namespace Blink3.API.Models;

public class AuthStatus
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? GlobalName { get; set; }
    public string? Locale { get; set; }
    public bool Authenticated { get; set; }
}