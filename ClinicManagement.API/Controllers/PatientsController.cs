using ClinicManagement.Application.Common.Exceptions;
using ClinicManagement.Application.Features.Patients.Commands.CreatePatient;
using ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;
using ClinicManagement.Application.Features.Patients.Queries.GetPatientById;
using ClinicManagement.Application.Features.Patients.Queries.SearchPatients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Create([FromBody] CreatePatientCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpGet]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Search([FromQuery] string? searchTerm, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SearchPatientsQuery(searchTerm), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Receptionist,Doctor,Patient")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPatientByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePatientCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            throw new ValidationException("Route ID does not match command ID.");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
