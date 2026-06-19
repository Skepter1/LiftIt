using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Models
{
    public class Uzytkownik
    {
        public int id { get; set; }
        public string login { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}
