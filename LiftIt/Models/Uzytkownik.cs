using System;
using System.Collections.Generic;
using System.Text;

namespace LiftIt.Models
{
    public class Uzytkownik
    {
        public string login { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public DateTime register_date { get; set; }
    }
}
