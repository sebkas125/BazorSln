using BazorProject.Server.Paging;
using BazorProject.Shared;
using BazorProject.Shared.Paging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BazorProject.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private string xmlpath = $"{Environment.CurrentDirectory}/GaestebuchEintraege.xml";
        private string filePath = Path.Combine(Environment.CurrentDirectory, "Files");
        private string filePathToDownscaled = Path.Combine(Environment.CurrentDirectory, "Files", "Small");

        [HttpGet]
        public IActionResult Get([FromQuery] PagingParameters parameters)
        {
            if (!System.IO.File.Exists(xmlpath))
            {
                return Ok();
            }

            List<Post> posts = getAll();
            posts = posts.Where(x => x.PostType == parameters.PostType).OrderByDescending(x => x.Date).ToList();
            PagedList<Post> pagedList = PagedList<Post>.ToPagedList(posts,
                    parameters.PageNumber,
                    parameters.PageSize);

            Parallel.ForEach(pagedList, entry =>
            {
                if (!string.IsNullOrEmpty(entry.Filename))
                {
                entry.Image = System.IO.File.Exists(Path.Combine(filePathToDownscaled, entry.Filename)) ? getImageAsByteArray(Path.Combine(filePathToDownscaled, entry.Filename)) : getImageAsByteArray(Path.Combine(filePath, entry.Filename));
        }
            });

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(pagedList.MetaData));
            return Ok(pagedList);
        }

        [HttpPost("Save")]
        public ActionResult<string> Save(Post neuerEintrag)
        {
            List<Post> gaestebuchEintraege = getAll();
            gaestebuchEintraege.Add(neuerEintrag);
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<Post>));
            StreamWriter wfile = new StreamWriter(xmlpath);
            writer.Serialize(wfile, gaestebuchEintraege);
            wfile.Close();
            return Content("");
        }

        private byte[] getImageAsByteArray(string pathToFile)
        {
            return System.IO.File.ReadAllBytes(pathToFile);
        }

        private List<Post> getAll()
        {
            List<Post> gaestebuchEintraege = new List<Post>();
            if (!System.IO.File.Exists(xmlpath))
            {
                return gaestebuchEintraege;
            }

            StreamReader sr = new StreamReader(xmlpath);
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(List<Post>));
            gaestebuchEintraege = (List<Post>)reader.Deserialize(sr);
            sr.Close();
            return gaestebuchEintraege;
        }
    }}