using ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Doctor")]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAppointmentsQuery(), cancellationToken);
        return Ok(result);
    }
}
