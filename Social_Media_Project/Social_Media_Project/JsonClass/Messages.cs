using System;

namespace Social_Media_Project.JsonClass
{
    internal class Messages
    {
        public int id { get; set; }
        public int fromID { get; set; }
        public int toID { get; set; }
        public string toName { get; set; }
        public string toImage { get; set; }
        public string fromName { get; set; }
        public string fromImage { get; set; }
        public string content { get; set; }
        public DateTime date { get; set; } = DateTime.Now;
    }
}
