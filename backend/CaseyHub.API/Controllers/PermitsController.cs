using System.Security.Claims;
using CaseyHub.API.Services;
using CaseyHub.Core.Interfaces;
using CaseyHub.Models.DTOs.Internal.Permit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNearbyPermits([FromQuery] string address, [FromQuery] int radiusKm = 5)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return BadRequest(new {message = "Address cannot be empty"});
        }
        try
        {
            var permits = await permitService.GetPermitsNearAddressAsync(address, radiusKm);    
            return Ok(permits);
        }catch(Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new {error = ex.Message});
        }
    }

    [HttpPost("savePermitToUser")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]   
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SavePermitToUser ([FromBody] SavePermitToUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ApplicationNumber))
        {
            return BadRequest(new {message = "Application Number cannot be empty"});
        }
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)|| !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new {message = "Invalid/Missing User Authentication"});
        }
        try
        {
            await permitService.SaveUserPermitAsync(userId, request.ApplicationNumber);
            return Ok(new {message = $"Permit {request.ApplicationNumber} has been added to user."});
        }catch (KeyNotFoundException ex)
        {
            return NotFound(new {message = ex.Message});
        }catch(Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new {message = "Unknown Error", ex.Message});
        }
    }

    [HttpGet("getUserSavedPermits")]
    [Authorize]
    [ProducesResponseType(typeof(PermitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserSavedPermits()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim)|| !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new {message = "Invalid/Missing User Authentication"});
        }

        try
        {
            List<PermitDto> permits = await permitService.GetUserSavedPermitsAsync(userId);
            return Ok(permits);
        }catch(Exception ex)
        {
           return StatusCode(StatusCodes.Status500InternalServerError, new{message = "Unknown Error", ex.Message});
        }
    }

}