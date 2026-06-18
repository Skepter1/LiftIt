using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Models
{
    public class StateService
    {
        public bool IsLoggedIn { get; set; } = false;

        public Uzytkownik CurrentUser { get; set; }
        public bool DarkTheme { get; set; } = true;
    }
}
