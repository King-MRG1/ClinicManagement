using ClinicManagement.Application;
using ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;
using ClinicManagement.Application.Features.MedicalRecords.Queries.GetMedicalRecords;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MedicalRecordsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Doctor)]
    public async Task<IActionResult> Create([FromBody] CreateMedicalRecordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Created($"/api/medical-records/{result.Id}", result);
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.Patient}")]
    public async Task<IActionResult> Get([FromQuery] Guid? patientId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMedicalRecordsQuery(patientId), cancellationToken);
        return Ok(result);
    }
}
