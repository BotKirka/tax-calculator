using Microsoft.AspNetCore.Mvc;
using TaxCalculatorLibrary.Models;

namespace CalculatorWeb.Controllers
{
    [ApiController]
    [Route("api/taxcalcus")]
    public class CalculatorController : Controller
    {
        [HttpGet]
        [Route("calculate")]
        public async Task<ActionResult> CalculateNettoAsync([FromQuery] SalaryData salary)
        {
            return Ok();
        }
    }
}
