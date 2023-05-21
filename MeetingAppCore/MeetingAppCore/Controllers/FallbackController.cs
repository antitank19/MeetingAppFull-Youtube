using MeetingAppCore.DebugTracker;
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
            Console.WriteLine("1."+new String('=', 50));
            Console.WriteLine("1.Web/Fallback: Index()"); 
            FunctionTracker.Instance().AddApiFunc("Web/Fallback: Index()");
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        }
    }
}
