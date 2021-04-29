using System;

namespace BazorProject.Shared
{
    public class GuestbookEntry
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public string Filename { get; set; }
        public byte[] Image { get; set; }
    }
}