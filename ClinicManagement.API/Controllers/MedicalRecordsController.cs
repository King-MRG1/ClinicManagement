using ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;
using ClinicManagement.Application.Features.MedicalRecords.Queries.GetPatientMedicalHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MedicalRecordsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("api/medical-records")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Create([FromBody] CreateMedicalRecordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Created(string.Empty, result);
    }

    [HttpGet("api/patients/{patientId:guid}/medical-records")]
    [Authorize(Roles = "Doctor,Patient")]
    public async Task<IActionResult> GetPatientMedicalHistory([FromRoute] Guid patientId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPatientMedicalHistoryQuery(patientId), cancellationToken);
        return Ok(result);
    }
}
