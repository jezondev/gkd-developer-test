using System.Net.Http.Headers;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Core;
using System.Text.Json;

namespace Computer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TravelController : ControllerBase
    {
        private readonly ITravelService _travelService;

        public TravelController(ITravelService travelService)
        {
            _travelService = travelService;
        }

        [HttpPost()]
        public async Task<IActionResult> Evaluate()
        {
            try
            {
                var file = Request.Form.Files[0];

                if (file.Length == 0)
                    return BadRequest();
                
                // TODO : (JES) -> Review and handle elsewhere and in a better way
                StreamReader reader = new StreamReader(file.OpenReadStream());
                string empirePlanJsonStr = await reader.ReadToEndAsync();
                EmpirePlan empirePlan = JsonSerializer.Deserialize<EmpirePlan>(empirePlanJsonStr);
                
                var result = await _travelService.EvaluateTravelOddsAsync(empirePlan);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}