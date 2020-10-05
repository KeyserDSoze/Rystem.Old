using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Web.Controllers
{
    public class RystemAjaxManagerController : Controller
    {
        public IActionResult ShowDocument([FromQuery] string url)
            => PartialView("ShowDocument", url);
        public IActionResult ShowImage([FromQuery] string url)
            => PartialView("ShowImage", url);
        public async Task<IActionResult> Download([FromQuery] string name, [FromQuery] IEnumerable<string> urls)
        {
            MemoryStream memoryStream = new MemoryStream();
            using ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
            using WebClient myWebClient = new WebClient();
            // Download the Web resource and save it into the current filesystem folder.
            List<byte[]> downloads = new List<byte[]>();
            foreach (var url in urls)
            {
                byte[] file = await myWebClient.DownloadDataTaskAsync(url);
                var entry = zipArchive.CreateEntry(url.Split('/').Last());
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(file);
            }
            zipArchive.Dispose();
            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream.ToArray(), "application/zip", $"{name}.zip");
        }
    }
}
