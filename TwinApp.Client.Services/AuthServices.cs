namespace TwinApp.Client.Services;
public class AuthService
{
    public bool IsLoggedIn { get; private set; } = false;

    public void Login() => IsLoggedIn = true;
    public void Logout() => IsLoggedIn = false;
}
