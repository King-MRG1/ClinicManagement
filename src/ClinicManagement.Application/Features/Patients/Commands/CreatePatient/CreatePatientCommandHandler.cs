using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs.Patients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, ApiResponse<PatientDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IClinicDbContext _context;
    private readonly ILogger<CreatePatientCommandHandler> _logger;

    public CreatePatientCommandHandler(
        IIdentityService identityService,
        IClinicDbContext context,
        ILogger<CreatePatientCommandHandler> logger)
    {
        _identityService = identityService;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PatientDto>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var password = string.IsNullOrWhiteSpace(request.Password) ? "Patient@123" : request.Password;

        var result = await _identityService.RegisterPatientAsync(
            request.Email,
            password,
            request.FullName,
            request.PhoneNumber,
            request.DateOfBirth,
            request.Gender,
            request.Address,
            cancellationToken);

        if (!result.Succeeded || result.User == null || !result.PatientId.HasValue)
        {
            _logger.LogWarning("Failed to create patient {Email}. Errors: {Errors}",
                request.Email, string.Join(", ", result.Errors));

            return ApiResponse<PatientDto>.Failed("Failed to create patient.", result.Errors);
        }

        _logger.LogInformation("Receptionist created new patient record with ID {PatientId}", result.PatientId.Value);

        var dto = new PatientDto
        {
            Id = result.PatientId.Value,
            UserId = result.User.Id,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Address = request.Address
        };

        return ApiResponse<PatientDto>.Succeeded(dto, "Patient created successfully.");
    }
}
