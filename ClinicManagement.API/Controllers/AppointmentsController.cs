using ClinicManagement.Application.Features.Appointments.Commands.CancelAppointment;
using ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicManagement.Application.Features.Appointments.Commands.RescheduleAppointment;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentById;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public record RescheduleRequest(DateTime NewAppointmentDateTime);

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
    [Authorize(Roles = "Receptionist,Patient")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpGet]
    [Authorize(Roles = "Receptionist,Doctor,Patient")]
    public async Task<IActionResult> Get(
        [FromQuery] DateOnly? date,
        [FromQuery] Guid? doctorId,
        [FromQuery] Guid? patientId,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] bool? upcomingOnly,
        CancellationToken cancellationToken)
    {
        var query = new GetAppointmentsQuery
        {
            Date = date,
            DoctorId = doctorId,
            PatientId = patientId,
            Status = status,
            UpcomingOnly = upcomingOnly
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Receptionist,Doctor,Patient")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAppointmentByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/reschedule")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Reschedule(
        [FromRoute] Guid id,
        [FromBody] RescheduleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RescheduleAppointmentCommand
        {
            AppointmentId = id,
            NewAppointmentDateTime = request.NewAppointmentDateTime
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = "Receptionist,Patient")]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelAppointmentCommand(id), cancellationToken);
        return Ok(result);
    }
}
