using BazorProject.Shared.Paging;
using System.Collections.Generic;

namespace BazorProject.Client.Features
{
    public class PagingResponse<T> where T : class
    {
        public List<T> Items { get; set; }
        public MetaData MetaData { get; set; }
    }
}