using System;

namespace Social_Media_Project
{
    internal class Posts
    {
        public int id { get; set; } = 0;
        public int userID { get; set; }
        public string name { get; set; }
        public string tag { get; set; }
        public string userImage { get; set; }
        public string imageURL { get; set; }
        public string text { get; set; }
        public DateTime date { get; set; } = DateTime.Now;
    }
}
