using System;

namespace BazorProject.Shared
{
    public class Post
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public string Filename { get; set; }
        public byte[] Image { get; set; }
        public PostType PostType { get; set; }
    }

    public enum PostType
    {
        Guestbook,
        Blog
    }
}