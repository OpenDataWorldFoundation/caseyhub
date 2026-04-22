using CaseyHub.API.Services;
using CaseyHub.Core.Interfaces;
using CaseyHub.Models.DTOs.Internal.Permit;
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

    [HttpPost("addPermitByApplicationNumber")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddPermitByApplicationNumber([FromBody] AddPermitRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ApplicationNumber))
            return BadRequest(new { message = "Application number cannot be empty." });
        try
        {
            await permitService.AddPermitByAppNumberToDBAsync(request.ApplicationNumber);
            return CreatedAtAction(
                nameof(GetPermitByApplicationNumber),
                new { applicationNumber = request.ApplicationNumber },
                new { message = $"Permit '{request.ApplicationNumber}' successfully added." }
            );
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
    
    [HttpPost("enrichSaveAllPermits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEnrichSaveAllPermitsFromCouncil()
    {
        try
        {
            await permitService.EnrichSaveAllPermitsAsync();
            return Ok(new { message = "All permits fetched, enriched and saved." });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    [HttpPost("syncPermitsFromCouncil")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SyncPermitsFromCouncil()
    {
        try
        {
            await permitService.SyncPermitsAsync();
            return Ok(new { message = "Sync complete." });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    [HttpGet("getNearbyPermits")]
    [ProducesResponseType(typeof(PermitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNearbyPermits([FromQuery] string address, [FromQuery] int radiusKm = 5)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return BadRequest(new {message = "Address cannot be empty"});
        }
        var permits = await permitService.GetPermitsNearAddressAsync(address, radiusKm);
        return Ok(permits);
    }
}