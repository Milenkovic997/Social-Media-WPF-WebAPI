using System.ComponentModel.DataAnnotations;

namespace Social_Media_API
{
    public class Users
    {
        [Key]
        public int id { get; set; } = 0;

        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }

        public string name { get; set; } = string.Empty;
        public string userTag { get; set; }
        public string imageURL { get; set; } = null;
        public string imageURLBG { get; set; } = null;
        public string bio { get; set; } = string.Empty;
        public string livesIn { get; set; } = string.Empty;
        public string relationship { get; set; } = string.Empty;

        public DateTime joined { get; set; }
        public bool privateAccount { get; set; } = false;
    }

    public class Following
    {
        [Key]
        public int id { get; set; } = 0;

        public int userID { get; set; } = 0;

        public int followingID { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public string image { get; set; } = string.Empty;
    }

    public class Messages
    {
        [Key]
        public int id { get; set; } = 0;

        public int fromID { get; set; } = 0;
        public int toID { get; set; } = 0;
        public string toName { get; set; } = string.Empty;
        public string toImage { get; set; } = string.Empty;
        public string fromName { get; set; } = string.Empty;
        public string fromImage { get; set; } = string.Empty;
                    
        public string content { get; set; } = string.Empty;
        public DateTime date { get; set; } = DateTime.Now;
    }
}
