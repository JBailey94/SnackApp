using System.Security.Claims;

namespace SnackApp.UI.Services {
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState authenticationState;

        public CustomAuthenticationStateProvider(CustomAuthenticationService service)
        {
            authenticationState = new AuthenticationState(service.CurrentUser);

            service.UserChanged += (newUser) =>
            {
                authenticationState = new AuthenticationState(newUser);
                NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
            };
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(authenticationState);
    }
}
