using ClinicManagement.Application.Features.Prescriptions.Commands.CreatePrescription;
using ClinicManagement.Application.Features.Prescriptions.Queries.GetPatientPrescriptions;
using ClinicManagement.Application.Features.Prescriptions.Queries.GetPrescriptionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
public class PrescriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PrescriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("api/prescriptions")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpGet("api/patients/{patientId:guid}/prescriptions")]
    [Authorize(Roles = "Doctor,Patient")]
    public async Task<IActionResult> GetPatientPrescriptions([FromRoute] Guid patientId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPatientPrescriptionsQuery(patientId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("api/prescriptions/{id:guid}")]
    [Authorize(Roles = "Doctor,Patient")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPrescriptionByIdQuery(id), cancellationToken);
        return Ok(result);
    }
}
