using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Interfaces
{
    internal interface IRejestracjaView
    {
        string Fullname { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        string PasswordConfirm { get; set; }

        event Action SignUpUser;
    }
}
