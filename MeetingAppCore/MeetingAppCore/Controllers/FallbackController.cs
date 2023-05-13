using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Controllers
{
    public class FallbackController : Controller
    {
        public IActionResult Index()
        {
            Console.WriteLine(new String('=', 10));
            Console.WriteLine("Web/Fallback: Index()");
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        }
    }
}
