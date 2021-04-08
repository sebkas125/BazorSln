using BazorProject.Shared;
using Microsoft.AspNetCore.Http;
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
    public class GuestbookController : ControllerBase
    {
        string path = $"{Environment.CurrentDirectory}/GaestebuchEintraege.xml";

        [HttpGet]
        public List<GuestbookEntry> Get()
        {
            List<GuestbookEntry> gaestebuchEintraege = new List<GuestbookEntry>();
            if (!System.IO.File.Exists(path))
            {
                return gaestebuchEintraege;
            }

            StreamReader sr = new StreamReader(path);
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(List<GuestbookEntry>));
            gaestebuchEintraege = (List<GuestbookEntry>)reader.Deserialize(sr);
            foreach (GuestbookEntry entry in gaestebuchEintraege)
            {
                entry.Image = getImageAsByteArray(entry.PathToFile);
            }

            sr.Close();
            return gaestebuchEintraege;
        }

        private byte[] getImageAsByteArray(string pathToFile)
        {
            return System.IO.File.ReadAllBytes(pathToFile);
        }

        [HttpPost("Save")]
        public ActionResult<string> Save(GuestbookEntry neuerEintrag)
        {
            List<GuestbookEntry> gaestebuchEintraege = Get();
            gaestebuchEintraege.Add(neuerEintrag);
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<GuestbookEntry>));
            StreamWriter wfile = new StreamWriter(path);
            writer.Serialize(wfile, gaestebuchEintraege);
            wfile.Close();
            return Content("");
        }
    }
}
