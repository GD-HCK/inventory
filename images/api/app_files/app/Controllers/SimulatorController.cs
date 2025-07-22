using DataLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    [ApiController] // marks the controller behaviour for Api
    [Route("[controller]")] // sets the api  route to match the name of the class without Controller so /weatherforecast
    [AllowAnonymous]
    public class SimulatorController(ISimulatorHelper simulatortHelper) : ControllerBase
    {
        private readonly ISimulatorHelper _simulatortHelper = simulatortHelper;

        [HttpGet("{code}")]
        public ActionResult<JsonContent> SimulateHttpResponse(int code)
        {
            // Simulate a response based on the provided code
            var response = new
            {
                Key = "status",
                Value = code.ToString()
            };

            // Return the simulated response
            return StatusCode(code, response);
        }

        [HttpGet()]
        public ActionResult<JsonContent> SimulateRandomHttpResponse()
        {
            // Simulate a response based on the provided code
            var result = _simulatortHelper.SimulateRandomHttpResponse();

            var code = int.Parse(result.Where(x => x.Key == "code").FirstOrDefault().Value);

            // Return the simulated response
            return StatusCode(code, result);
        }
    }
}
