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

            var testServiceImpl = (TestService)testService;

            //return await testServiceImpl.GetNames(null);
            //return await testServiceImpl.GetAllNames();

            if (param.Id == 0)
            {
                await testServiceImpl.UpdateNames();
                return "ok";
            }
            //var result1 = await testServiceImpl.GetNames(param.Id);
            //return await testServiceImpl.GetNames(new TestServiceParam
            //{
            //    Id = param.Id ?? 1,
            //    Count = param.Count,
            //    Name = param.Name ?? "asd"
            //});
            return await testServiceImpl.GetNames(new TestServiceParam
            {
                Id = param.Id,
                Count = param.Count,
                Name = param.Name,
                Param = param.Id.HasValue ? new TestServiceParam
                {
                    Id = param.Id,
                    Count = param.Count,
                    Name = param.Name,
                } : null
            });
        }
    }
}
