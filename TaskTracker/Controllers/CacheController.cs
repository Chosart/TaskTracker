﻿using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> SetCacheValue(string key, string value, int minutes)
        {
            await _redisCacheService.SetCacheValueAsync(key, value, TimeSpan.FromMinutes(minutes));
            return Ok();
        }
    }
}
