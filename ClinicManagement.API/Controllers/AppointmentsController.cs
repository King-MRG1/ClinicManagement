using ClinicManagement.Application;
using ClinicManagement.Application.Features.Appointments.Commands.CancelAppointment;
using ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Receptionist)]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Created($"/api/appointments/{result.Id}", result);
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Receptionist},{Roles.Patient}")]
    public async Task<IActionResult> Get(
        [FromQuery] Guid? doctorId,
        [FromQuery] Guid? patientId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAppointmentsQuery(doctorId, patientId), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = $"{Roles.Receptionist},{Roles.Patient}")]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelAppointmentCommand(id), cancellationToken);
        return Ok(result);
    }
}
