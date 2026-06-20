using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Interfaces
{
    internal interface IEditProfileView
    {
        string Name { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        public void SuccesfulModification(string message);
        event Action ModificationRequested;
    }
}
