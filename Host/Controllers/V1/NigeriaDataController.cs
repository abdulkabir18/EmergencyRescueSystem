using Application.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class NigeriaDataController : ControllerBase
    {
        [HttpGet("states")]
        public IActionResult GetStates()
        {
            return Ok(NigeriaData.States);
        }

        [HttpGet("cities/{state}")]
        public IActionResult GetCities(string state)
        {
            if (NigeriaData.Cities.TryGetValue(state, out var cities))
                return Ok(cities);

            return NotFound(new { Message = $"No cities found for state: {state}" });
        }

        [HttpGet("lgas/{state}")]
        public IActionResult GetLGAs(string state)
        {
            if (NigeriaData.LGAs.TryGetValue(state, out var lgas))
                return Ok(lgas);

            return NotFound(new { Message = $"No LGAs found for state: {state}" });
        }
    }
}
