﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BazorProject.Shared
{
    public class GuestbookEntry
    {
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public string PathToFile { get; set; }
    }
}
