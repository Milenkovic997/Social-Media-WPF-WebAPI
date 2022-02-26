using System;
using System.Collections.Generic;

namespace Social_Media_Project
{
    internal class Users
    {
        public int id { get; set; } = 0;
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string userTag { get; set; }
        public string imageURL { get; set; } = "https://i.imgur.com/Gtjeuu5.png";
        public string imageURLBG { get; set; } = string.Empty;
        public string bio { get; set; } = string.Empty;
        public string livesIn { get; set; } = string.Empty;
        public string relationship { get; set; } = string.Empty;
        public DateTime joined { get; set; } = DateTime.Now;
        public int unreadMessages { get; set; } = 0;
        public bool privateAccount { get; set; } = false;
        public bool dnd { get; set; } = false;
    }
}
