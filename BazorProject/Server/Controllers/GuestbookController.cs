using BazorProject.Server.Paging;
using BazorProject.Shared;
using BazorProject.Shared.Paging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private string xmlpath = $"{Environment.CurrentDirectory}/GaestebuchEintraege.xml";
        private string filePathToDownscaled = Path.Combine(Environment.CurrentDirectory, "Files", "Small");

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PagingParameters parameters)
        {
            if (!System.IO.File.Exists(xmlpath))
            {
                return Ok();
            }

            List<GuestbookEntry> gaestebuchEintraege = getAll();
            gaestebuchEintraege = gaestebuchEintraege.OrderByDescending(x => x.Date).ToList();
            PagedList<GuestbookEntry> pagedList = PagedList<GuestbookEntry>.ToPagedList(gaestebuchEintraege,
                    parameters.PageNumber,
                    parameters.PageSize);
            foreach (GuestbookEntry entry in pagedList)
            {
                entry.Image = string.IsNullOrEmpty(entry.Filename) || !System.IO.File.Exists(Path.Combine(filePathToDownscaled, entry.Filename)) ? null : getImageAsByteArray(Path.Combine(filePathToDownscaled, entry.Filename));
            }

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(pagedList.MetaData));
            return Ok(pagedList);
        }

        [HttpPost("Save")]
        public ActionResult<string> Save(GuestbookEntry neuerEintrag)
        {
            List<GuestbookEntry> gaestebuchEintraege = getAll();
            gaestebuchEintraege.Add(neuerEintrag);
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<GuestbookEntry>));
            StreamWriter wfile = new StreamWriter(xmlpath);
            writer.Serialize(wfile, gaestebuchEintraege);
            wfile.Close();
            return Content("");
        }

        private byte[] getImageAsByteArray(string pathToFile)
        {
            return System.IO.File.ReadAllBytes(pathToFile);
        }

        private List<GuestbookEntry> getAll()
        {
            List<GuestbookEntry> gaestebuchEintraege = new List<GuestbookEntry>();
            if (!System.IO.File.Exists(xmlpath))
            {
                return gaestebuchEintraege;
            }

            StreamReader sr = new StreamReader(xmlpath);
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(List<GuestbookEntry>));
            gaestebuchEintraege = (List<GuestbookEntry>)reader.Deserialize(sr);
            sr.Close();
            return gaestebuchEintraege;
        }
    };
}