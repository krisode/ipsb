using IPSB.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static IPSB.Utils.Constants;

namespace IPSB.Controllers
{
    [Route("api/v1.0/test-cache")]
    [ApiController]
    public class TestCacheController : Controller
    {
        private readonly ICacheStore _cacheService;

        public TestCacheController(ICacheStore cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet("{key}")]
        public async Task<ActionResult> GetByKey(string key)
        {
            return Ok(await _cacheService.GetByKey(key));
        }

        [HttpGet]
        public ActionResult GetAll()
        {
            return Ok(_cacheService.GetAllKeys());
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAll()
        {
            if (await _cacheService.RemoveAll())
            {
                return Ok("Removed all successfully!");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

    }
}
