using Microsoft.AspNetCore.Mvc;
using SpringCaching.Tests;

namespace SpringCaching.TestWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<object>> Get([FromServices] ITestService testService, [FromQuery] TestServiceParam param)
        {
            //return await testService.GetNames(null);
            //return await testService.GetAllNames();
            if (param.Id == 0)
            {
                await testService.UpdateNames();
            }
            return await testService.GetNames(new TestServiceParam
            {
                Id = param.Id ?? 1,
                Name = param.Name ?? "asd"
            });
        }
    }
}
