using MeetingAppCore.DebugTracker;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MeetingAppCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunctionController : ControllerBase
    {
        // GET: api/<FunctionController>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(FunctionTracker.Instance().All);
        }
        [HttpGet("Clear")]
        public  IActionResult Clear ()
        {
            FunctionTracker.Instance().All.Clear();
            FunctionTracker.Instance().Api.Clear();
            FunctionTracker.Instance().Service.Clear();
            FunctionTracker.Instance().Repo.Clear();
            FunctionTracker.Instance().Hub.Clear();    
            FunctionTracker.Instance().Tracker.Clear();
            return Ok();
        }

        // GET: api/<FunctionController>
        [HttpGet("Api")]
        public IActionResult GetApi()
        {
            return Ok(FunctionTracker.Instance().Api);
        }
        [HttpGet("Service")]
        public IActionResult GetService()
        {
            return Ok(FunctionTracker.Instance().Service);
        }
        [HttpGet("Repo")]
        public IActionResult GetRepo()
        {
            return Ok(FunctionTracker.Instance().Repo);
        }
        [HttpGet("Hub")]
        public IActionResult GetHub()
        {
            return Ok(FunctionTracker.Instance().Hub);
        }
        [HttpGet("Tracker")]
        public IActionResult GetTracker()
        {
            return Ok(FunctionTracker.Instance().Tracker);
        }
    }
}
