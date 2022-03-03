using System.ComponentModel.DataAnnotations;

namespace Social_Media_API
{
    public class Posts
    {
        [Key]
        public int id { get; set; } = 0;

        public int userID { get; set; }
        public string name { get; set; } = string.Empty;
        public string tag { get; set; } = string.Empty;
        public string userImage { get; set; } = string.Empty;
        public string imageURL { get; set; } = string.Empty;
        public byte[] imageFile { get; set; } = Array.Empty<byte>();
        public string text { get; set; } = string.Empty;
        public DateTime date { get; set; } = DateTime.Now;
    }
}
