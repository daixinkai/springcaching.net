using Microsoft.AspNetCore.Mvc;
using SpringCaching.Tests;

namespace SpringCaching.TestWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<object>> Get([FromServices] ITestService testService)
        {
            //return await testService.GetNames(null);
            return await testService.GetAllNames();
        }
    }
}
