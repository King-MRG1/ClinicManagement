# Clinic Management System API

A small and simple practice project built with **.NET 10** to learn and practice **Clean Architecture**, **CQRS (MediatR)**, **Entity Framework Core**, and **JWT Authentication**.

---

## 1. What the Project Is

This is an educational backend Web API simulating a simplified clinic workflow. It is intentionally kept small, readable, and focused on core concepts without unnecessary enterprise boilerplate, event sourcing, or generic repository abstractions.

---

## 2. Why Clean Architecture

Clean Architecture isolates core business rules from frameworks, databases, and UI concerns.

### Dependency Direction

```
Domain  <───  Application  <───  Infrastructure
  ▲                 ▲
  │                 │
  └───────────── API ┘
```

- **ClinicManagement.Domain**: Core entities and enums. Zero external package dependencies.
- **ClinicManagement.Application**: Business rules, CQRS commands/queries, MediatR handlers, DTOs, and interface contracts. Depends only on Domain.
- **ClinicManagement.Infrastructure**: EF Core `ClinicDbContext`, entity configurations, database migrations, seeding, and `JwtTokenService`. Depends on Application and Domain.
- **ClinicManagement.API**: Controllers, middleware, authentication wiring, Scalar, and OpenAPI configuration. Entry point of the application.

---

## 3. What CQRS Means in this Project

CQRS (**Command Query Responsibility Segregation**) separates operations that mutate state from operations that read state:

- **Commands** (Write): Mutate data, validate business constraints (e.g., duplicate time checks), and persist changes.
- **Queries** (Read): Fetch data without side effects, projecting directly into lightweight DTOs.

### Example Flow: Creating an Appointment

```
Client (HTTP POST /api/appointments)
  │
  ▼
AppointmentsController
  │  _mediator.Send(CreateAppointmentCommand)
  ▼
ValidationBehavior (FluentValidation checks required fields)
  │
  ▼
CreateAppointmentCommandHandler
  │
  ├─► Check doctor conflict in DB: (DoctorId == req.DoctorId && Date == req.Date && Status != Cancelled)
  │     └─► If conflict: throws ConflictException (HTTP 409)
  │
  ├─► Creates Appointment entity (Status = Scheduled)
  │
  ▼
ClinicDbContext.SaveChangesAsync()
  │
  ▼
SQL Server Database (Enforces unique filtered index on DoctorId + AppointmentDate)
  │
  ▼
Returns AppointmentDto (HTTP 201 Created)
```

---

## 4. Project Structure

```text
CMS/
├── ClinicManagement.slnx
├── ClinicManagement.Domain/
│   ├── Entities/
│   │   ├── ApplicationUser.cs
│   │   ├── Doctor.cs
│   │   ├── Patient.cs
│   │   ├── Appointment.cs
│   │   └── MedicalRecord.cs
│   └── Enums/
│       └── AppointmentStatus.cs
├── ClinicManagement.Application/
│   ├── Common/
│   │   ├── Behaviors/ValidationBehavior.cs
│   │   ├── Exceptions/ (NotFound, Conflict, Forbidden, Unauthorized, Validation)
│   │   ├── Interfaces/ (IClinicDbContext, ICurrentUserService, IJwtTokenService)
│   │   └── Roles.cs
│   ├── DTOs/
│   │   ├── Auth/AuthResponseDto.cs
│   │   ├── Patients/PatientDto.cs
│   │   ├── Appointments/AppointmentDto.cs
│   │   └── MedicalRecords/MedicalRecordDto.cs
│   └── Features/
│       ├── Authentication/Commands/Login/
│       ├── Patients/Commands & Queries (Create, Update, Get)
│       ├── Appointments/Commands & Queries (Create, Cancel, Get)
│       └── MedicalRecords/Commands & Queries (Create, Get)
├── ClinicManagement.Infrastructure/
│   ├── Persistence/
│   │   ├── ClinicDbContext.cs
│   │   ├── ClinicDbSeeder.cs
│   │   └── Configurations/ (ApplicationUser, Doctor, Patient, Appointment, MedicalRecord)
│   ├── Migrations/
│   └── Services/JwtTokenService.cs
└── ClinicManagement.API/
    ├── Controllers/ (Auth, Patients, Appointments, Doctors, MedicalRecords)
    ├── Middleware/ExceptionHandlingMiddleware.cs
    ├── Services/CurrentUserService.cs
    ├── Program.cs
    ├── ClinicManagement.API.http
    ├── appsettings.json
    └── appsettings.Development.json
```

---

## 5. Roles and Capabilities

The system has exactly three roles (no Admin role):

1. **Doctor**:
   - View their own scheduled appointments (`GET /api/doctors/appointments`).
   - Create medical records for patients who have an appointment with them (`POST /api/medical-records`).
   - View medical records of their patients (`GET /api/medical-records`).
2. **Receptionist**:
   - Manage patients: Create, view, and update patient information (`POST`, `GET`, `PUT /api/patients`).
   - Manage appointments: Schedule new appointments, view all appointments, and cancel appointments (`POST`, `GET`, `PUT /api/appointments`).
3. **Patient**:
   - View their own appointments (`GET /api/appointments`).
   - Cancel their own upcoming appointment (`PUT /api/appointments/{id}/cancel`).
   - View their own medical records (`GET /api/medical-records`).

---

## 6. Main API Endpoints

| HTTP Method | Route | Allowed Roles | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | *Anonymous* | Authenticate user and receive JWT Bearer token |
| `POST` | `/api/patients` | `Receptionist` | Register a new patient in the clinic |
| `GET` | `/api/patients` | `Receptionist` | Search/list patients |
| `PUT` | `/api/patients/{id}` | `Receptionist` | Update an existing patient's details |
| `POST` | `/api/appointments` | `Receptionist` | Schedule an appointment for a patient |
| `GET` | `/api/appointments` | `Receptionist`, `Patient` | List appointments (Patients restricted to their own) |
| `PUT` | `/api/appointments/{id}/cancel` | `Receptionist`, `Patient` | Cancel an appointment (Patients can only cancel own) |
| `GET` | `/api/doctors/appointments` | `Doctor` | View appointments assigned to the logged-in doctor |
| `POST` | `/api/medical-records` | `Doctor` | Add medical record for a patient with an appointment |
| `GET` | `/api/medical-records` | `Doctor`, `Patient` | View medical records (Patient restricted to own) |

---

## 7. Seed Accounts

When the application database is created and seeded, the following test accounts are available:

| Role | Email | Password | Details |
| :--- | :--- | :--- | :--- |
| **Doctor** | `doctor@clinic.com` | `Password123!` | Dr. Alice Smith (Cardiology) |
| **Receptionist** | `receptionist@clinic.com` | `Password123!` | Bob Receptionist |
| **Patient 1** | `patient1@clinic.com` | `Password123!` | John Doe |
| **Patient 2** | `patient2@clinic.com` | `Password123!` | Jane Smith |

> **Note**: Any new patients registered by the Receptionist via `POST /api/patients` are automatically initialized with the default password `Password123!`.

---

## 8. Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server / LocalDB

### 1. Configure the JWT Secret

Development secrets are securely stored using `dotnet user-secrets` rather than hardcoding in source control:

```bash
dotnet user-secrets set "Jwt:Key" "SuperSecretClinicManagementPracticeProjectJwtKey2026!#" --project ClinicManagement.API
```

> The application validates that `Jwt:Key` is provided, not set to the placeholder string, and is at least 32 characters long.

### 2. Database Migrations

Migrations have been prepared and ready to apply to SQL Server:

```bash
dotnet ef database update --project ClinicManagement.Infrastructure --startup-project ClinicManagement.API
```

*(When SQL Server is accessible, migrations and seeding also run automatically on application startup).*

### 3. Run the Application

```bash
dotnet run --project ClinicManagement.API
```

---

## 9. API Documentation & Interactive Testing (Scalar)

The project uses **Scalar** powered by native .NET 10 OpenAPI support:

- **Scalar Interactive UI**: [`http://localhost:5222/scalar`](http://localhost:5222/scalar)
- **OpenAPI JSON Document**: [`http://localhost:5222/openapi/v1.json`](http://localhost:5222/openapi/v1.json)

### Authenticating in Scalar:

1. Send a request to `POST /api/auth/login` using one of the seed accounts above.
2. Copy the `token` value from the JSON response.
3. In Scalar, click the **Authorize / Bearer** button in the top right.
4. Paste the token into the Bearer token field. All subsequent requests will automatically include the authorization header.
