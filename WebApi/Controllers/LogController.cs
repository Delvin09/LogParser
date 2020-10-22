using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogRepository _logRepository;

        public LogController(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        [HttpGet("hosts")]
        public async Task<ActionResult<IEnumerable<string>>> GetHosts(int count = 10, DateTime? start = null, DateTime? end = null)
        {
            if (end < start || count <= 0)
                return BadRequest();

            return Ok(await _logRepository.GetFrequentlyHosts(count, start, end));
        }

        [HttpGet("routes")]
        public async Task<ActionResult<IEnumerable<string>>> GetRoutes(int count = 10, DateTime? start = null, DateTime? end = null)
        {
            if (end < start || count <= 0)
                return BadRequest();

            return Ok(await _logRepository.GetFrequentlyRoutes(count, start, end));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get(DateTime? start = null, DateTime? end = null, int offset = 0, int limit = 10)
        {
            if (end < start || offset < 0 || limit <= 0)
                return BadRequest();

            return Ok(await _logRepository.GetLogs(offset, limit, start, end));
        }
    }
}
