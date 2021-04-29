using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BazorProject.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FiledownloadController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> DownloadFile([FromQuery] string filename)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Files", filename);
            byte[] fileBytes = getFile(path);
            FileContentResult fileContentResult = File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, path);
            fileContentResult.FileDownloadName = filename;
            return fileContentResult;
        }

        private byte[] getFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }
    }
}