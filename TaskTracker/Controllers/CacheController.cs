using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Services;

namespace TaskTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        private readonly IRedisCacheService _redisCacheService;

        public CacheController(IRedisCacheService redisCacheService)
        {
            _redisCacheService = redisCacheService;
        }

        [HttpPost("set-cache")]
        public async Task<IActionResult> SetCacheValue([FromBody] CacheRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Key) || string.IsNullOrEmpty(request.Value))
            {
                return BadRequest("Key and value are required.");
            }

            await _redisCacheService.SetCacheValueAsync(request.Key, request.Value, TimeSpan.FromMinutes(request.Minutes));
            return Ok();
        }

        [HttpGet("get-cache")]
        public async Task<IActionResult> GetCacheValue(string key)
        {
            var value = await _redisCacheService.GetCacheValueAsync(key);

            if (value == null)
            {
                return NotFound();
            }

            return Ok(value);
        }

        [HttpDelete("remove-cache")]
        public async Task<IActionResult> RemoveCacheValue(string key)
        {
            await _redisCacheService.RemoveCacheValueAsync(key);
            return NoContent();
        }

        [HttpGet("exists-cache")]
        public async Task<IActionResult> KeyExists(string key)
        {
            var exists = await _redisCacheService.KeyExistsAsync(key);
            return Ok(exists);
        }
    }
}
