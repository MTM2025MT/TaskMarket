using Microsoft.AspNetCore.Mvc;
using TaskMarket.Factories;
using TaskMarket.Handlers;

namespace TaskMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractorController : ProblemHandler
    {
        private ContractorHandler _contractorHandler;
        public ContractorController(ContractorHandler contractorHandler)
        {
            _contractorHandler = contractorHandler;
        }

        [HttpGet("GetBestContractors")]
        public async Task<IActionResult> GetBestContractors(int? count)
        {
            var result = await _contractorHandler.GetBestContractors(count ?? 5);
            return result.Match<IActionResult>(
                contractors => Ok(contractors),
                failure => Problem(failure)
            );
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetContractorById(int id)
        {
            var boxresult = await _contractorHandler.GetByIdAsync(id);

            return boxresult.Match<IActionResult>(
                contractor => Ok(ContractorProfileFactory.Create(contractor)),
                failure => Problem(failure)
            );
        }
    }
}
