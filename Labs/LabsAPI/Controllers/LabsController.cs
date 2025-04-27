using LabsAPI.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace LabsAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LabsController : ControllerBase
{
    private readonly IProcessHandler _processHandler;

    public LabsController(IProcessHandler processHandler)
    {
        _processHandler = processHandler;
    }

    [HttpGet(Name = "GetString")]
    public async Task<IActionResult> Get(string input, bool isQuickSort)
    {
        var res = await _processHandler.GetResultAsync(input, isQuickSort);

        if (!string.IsNullOrEmpty(res.Error))
        {
            return BadRequest(res.Error);
        }

        return Ok(res);
    }
}
