using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafePOS.DTO
{
    public class Account
    {
        public static string CurrentUserName { get; set; } = "";
        public static string CurrentDisplayName { get; set; } = "";
        public static int CurrentUserType { get; set; } = 0;
        public static int CurrentUserStaffId { get; set; } = 0;

        public string UserName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int Type { get; set; } = 0;
        public string Password { get; set; }


    }
}
