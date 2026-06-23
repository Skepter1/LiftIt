using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Interfaces
{
    public interface ILogowanieView
    {
        string Email { get; set; }
        string Password { get; set; }
        void ShowSignInError(string message);
        void RedirectHomePage();

        event Action SignInUser;
        event Action LogoutUser;
    }
}