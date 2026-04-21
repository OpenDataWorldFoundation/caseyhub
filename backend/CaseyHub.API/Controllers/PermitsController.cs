using CaseyHub.API.Services;
using CaseyHub.Core.Interfaces;
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

    [HttpGet("addPermitByAppNumber/{applicationNumber}")]
    public async Task<IActionResult> AddPermitByApplicationNumber(string applicationNumber)
    {
        if (string.IsNullOrWhiteSpace(applicationNumber))
        {
            return BadRequest(new {message = "Application Number Cannot Be Empty."});
        }
        try
        {
            await permitService.AddPermitByAppNumberToDBAsync(applicationNumber);    
            return Ok(new{message = $"Permit '{applicationNumber}' succesfully added"});
        }catch(Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new {message = $"An error occured. See details: {ex.Message}"});
        }
    }
    
    [HttpGet("getEnrichSaveAllPermitsFromCouncil")]
    public async Task<IActionResult> GetEnrichSaveAllPermitsFromCouncil()
    {
        try
        {
            await permitService.GetEnrichSaveAllPermitsAsync();
            return Ok(new{message = "SAVED LITERALLY EVERYTHING FROM COUNCIL TO DB!!" });
        }catch(Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new {message = $"An error occured. See details: {ex.Message}"});
        }
    }

    [HttpGet("syncPermitsFromCouncil")]
    public async Task<IActionResult> SyncPermitsFromCouncil()
    {
        try
        {
            await permitService.SyncPermitsAsync();
            return Ok(new{message = "Sync Complete :)" });
        }catch(Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new {message = $"An error occured. See details: {ex.Message}"});
        }
    }
}