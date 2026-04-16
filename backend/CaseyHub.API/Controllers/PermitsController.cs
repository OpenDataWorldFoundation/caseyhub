using CaseyHub.API.Services;
using CaseyHub.Models.DTOs.Internal;
using Microsoft.AspNetCore.Mvc;

namespace CaseyHub.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PermitsController(IPermitService permitService) : ControllerBase
{
    [HttpGet("getPermitByAppNumber/{applicationNumber}")]
    [ProducesResponseType(typeof(PermitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPermitByApplicationNumber(string applicationNumber)
    {
        if (string.IsNullOrWhiteSpace(applicationNumber))
        {
            return BadRequest(new {message = "Application Number Cannot Be Empty."});
        }

        var permit = await permitService.GetPermitByAppNumberAsync(applicationNumber);
        if(permit == null)
        {
            return NotFound(new {message = $"Permit '{applicationNumber}' not found in council records."});
        }
        return Ok(permit);
    }
}