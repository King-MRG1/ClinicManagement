using ClinicManagement.Application.Features.Doctors.Queries.GetDoctorAvailability;
using ClinicManagement.Application.Features.Doctors.Queries.GetDoctorById;
using ClinicManagement.Application.Features.Doctors.Queries.GetDoctors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDoctors(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDoctorsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDoctorById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDoctorByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/availability")]
    public async Task<IActionResult> GetDoctorAvailability(
        [FromRoute] Guid id,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _mediator.Send(new GetDoctorAvailabilityQuery(id, targetDate), cancellationToken);
        return Ok(result);
    }
}
