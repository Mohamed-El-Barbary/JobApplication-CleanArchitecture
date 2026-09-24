# Job Application Management System

A production-oriented **Job Application Management System** built with **ASP.NET Core Web API**, **Clean Architecture**, **CQRS**, **MediatR**, **Entity Framework Core**, **SQL Server**, **ASP.NET Core Identity**, **JWT Authentication**, **Cloudinary**, and **Hangfire**.

The system manages the complete recruitment workflow between **Candidates** and **Recruiters**, including job management, applications, interviews, candidate profiles, CV management, authentication, notifications, and dashboards.

---

# Features

## Authentication & Account Security

The system provides a complete authentication and account-security lifecycle.

### Authentication

* User registration
* User login
* JWT access tokens
* Refresh tokens
* Refresh token rotation/revocation
* Logout
* Current authenticated user
* Change password

### Email & Account Security

* Email verification
* Resend verification email
* Forgot password
* Password reset
* Secure Identity token generation
* Email notifications using **Resend**

### Authentication Endpoints

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
POST /api/auth/revoke-refresh-token
GET  /api/auth/me
POST /api/auth/change-password

POST /api/auth/forgot-password
POST /api/auth/reset-password

POST /api/auth/verify-email
POST /api/auth/resend-verification-email
```

---

# Candidate

Candidates can manage their profiles and participate in the recruitment process.

### Candidate Features

* Register and authenticate
* Manage personal profile
* Upload CV
* Replace CV
* Delete CV
* Apply for open jobs
* View submitted applications
* View application details
* Update application information while editable
* Cancel applications
* Track application status
* View scheduled interviews
* View application-related information through the candidate dashboard

---

# Candidate Profile

Candidates can manage their professional profile.

### Profile Information

Depending on the current domain model, the profile can contain:

* Full Name
* Phone
* Location
* Bio
* Skills
* Years of Experience
* LinkedIn
* GitHub
* Portfolio
* CV information

### Candidate Profile Endpoints

```http
GET    /api/candidates/me
PUT    /api/candidates/me

POST   /api/candidates/me/cv
DELETE /api/candidates/me/cv
```

The authenticated user's identity is used to determine the candidate.

The API does not accept a `CandidateId` from the client for these operations.

---

# CV Management

Candidate CV files are stored using **Cloudinary**.

The application does not store CV binary data inside SQL Server.

Instead, the Business Database stores CV metadata such as:

```text
CVPublicId
CVUrl
CVFileName
CVUploadedAt
```

### CV Upload Flow

```text
Candidate
    │
    ▼
API
    │
    ▼
MediatR
    │
    ▼
UploadCandidateCvCommand
    │
    ▼
ICloudinaryService
    │
    ▼
Cloudinary
    │
    ▼
CV Metadata
    │
    ▼
Business Database
```

Supported CV formats should be validated before upload.

---

# Job Management

Recruiters can manage the jobs they own.

### Job Features

* Create a job
* View jobs
* View job details
* Update a job
* Delete a job
* View own jobs
* Close a job

### Job Endpoints

```http
POST   /api/Jobs
GET    /api/Jobs
GET    /api/Jobs/{jobId}
PUT    /api/Jobs/{jobId}
DELETE /api/Jobs/{jobId}

GET    /api/Jobs/my-jobs

PATCH  /api/Jobs/{jobId}/close
```

### Job Lifecycle

```text
Open
  │
  │ Close
  ▼
Closed
```

Once a job is closed, new applications cannot be submitted.

Existing applications remain preserved.

---

# Job Applications

The **Job Application Management** module allows candidates to apply for jobs and recruiters to manage applications submitted to their jobs.

## Application Workflow

```text
Candidate
    │
    │ Apply
    ▼
Applied
    │
    ▼
UnderReview
    │
    ▼
Interview
    │
    ├──────────────► Accepted
    │
    └──────────────► Rejected
```

A candidate can also cancel an eligible application:

```text
Applied / UnderReview / Interview
              │
              ▼
          Cancelled
```

---

# Application Status

| Status        | Description                               |
| ------------- | ----------------------------------------- |
| `Applied`     | Candidate has submitted the application   |
| `UnderReview` | Recruiter is reviewing the application    |
| `Interview`   | Candidate has reached the interview stage |
| `Accepted`    | Candidate has been accepted               |
| `Rejected`    | Application has been rejected             |
| `Cancelled`   | Candidate cancelled the application       |

---

# Candidate Application Endpoints

### Apply for a Job

```http
POST /api/jobs/{jobId}/applications
```

Creates a new application for an open job.

Candidate identity and initial status are determined by the authenticated user and business rules.

Example:

```json
{
  "coverLetter": "I am interested in this position...",
  "resumeUrl": "https://example.com/resume.pdf",
  "yearsOfExperience": 1
}
```

---

### Get My Applications

```http
GET /api/job-applications/my-applications
```

Returns applications submitted by the authenticated candidate.

---

### Get Application

```http
GET /api/job-applications/{applicationId}
```

Allows an authorized candidate or recruiter to view an application.

---

### Update Application

```http
PUT /api/job-applications/{applicationId}
```

Updates candidate-provided information while the application remains editable.

---

### Cancel Application

```http
PATCH /api/job-applications/{applicationId}/cancel
```

Changes the application status to:

```text
Cancelled
```

The application is not physically deleted.

---

# Recruiter Application Endpoints

### Get Applications for a Job

```http
GET /api/jobs/{jobId}/applications
```

Returns applications submitted for a job owned by the authenticated recruiter.

---

### Update Application Status

```http
PATCH /api/job-applications/{applicationId}/status
```

Example:

```json
{
  "status": "UnderReview"
}
```

Recruiters can manage the recruitment status of applications belonging to their jobs.

Recruiters cannot use this endpoint to set an application to `Cancelled`.

---

# Interviews

The system supports scheduling and managing interviews for job applications.

One application can have multiple interviews, for example:

```text
Application
    │
    ├── HR Interview
    ├── Technical Interview
    └── Final Interview
```

### Interview Types

```text
Online
InPerson
Phone
```

### Interview Status

```text
Scheduled
Completed
Cancelled
Rescheduled
```

### Interview Endpoints

```http
POST  /api/job-applications/{applicationId}/interviews

GET   /api/job-applications/{applicationId}/interviews

GET   /api/interviews/{interviewId}

PUT   /api/interviews/{interviewId}

PATCH /api/interviews/{interviewId}/reschedule

PATCH /api/interviews/{interviewId}/cancel

GET   /api/interviews/my-interviews
```

Interview scheduling validates the related application and recruiter ownership before creating an interview.

---

# Candidate Dashboard

Candidates can access an aggregated dashboard containing information about their recruitment activity.

### Endpoint

```http
GET /api/dashboard/candidate
```

Possible dashboard statistics include:

```text
Total Applications
Applied Applications
Under Review Applications
Interview Applications
Accepted Applications
Rejected Applications
Cancelled Applications
Upcoming Interviews
```

Dashboard queries are designed to perform aggregation at the database level rather than loading unnecessary records into memory.

---

# Recruiter Dashboard

Recruiters can access an aggregated dashboard for their recruitment activity.

### Endpoint

```http
GET /api/dashboard/recruiter
```

Possible statistics include:

```text
Total Jobs
Open Jobs
Closed Jobs
Total Applications
Pending Applications
Interview Count
Accepted Applications
Rejected Applications
Upcoming Interviews
```

All dashboard data is restricted to the authenticated recruiter and their jobs.

---

# Notifications & Background Jobs

The system uses **Hangfire** for background processing.

Background jobs are used for operations that should not block the HTTP request.

Examples include:

* Application cancellation notifications
* Email notifications
* Verification emails
* Password reset emails
* Other asynchronous notification workflows

### Background Job Flow

```text
API Request
    │
    ▼
Application Service / Handler
    │
    ▼
Hangfire
    │
    ▼
Background Job
    │
    ▼
Notification / Email Service
```

Hangfire storage can be separated from the Business Database to isolate background-job infrastructure from core business data.

---

# Email Service

The application uses **Resend** for transactional email delivery.

Email operations include:

* Email verification
* Resend verification email
* Password reset

The Application layer depends on an email abstraction, while the Resend implementation belongs to Infrastructure.

```text
Application
    │
    ▼
IEmailService
    │
    ▼
Infrastructure
    │
    ▼
Resend
```

API credentials are stored through configuration/environment secrets and are never hard-coded.

---

# Authorization

The API uses:

* JWT Authentication
* Role-Based Authorization
* Ownership-based authorization

### Candidate

```text
POST   /api/jobs/{jobId}/applications
GET    /api/job-applications/my-applications
GET    /api/job-applications/{applicationId}
PUT    /api/job-applications/{applicationId}
PATCH  /api/job-applications/{applicationId}/cancel

GET    /api/candidates/me
PUT    /api/candidates/me
POST   /api/candidates/me/cv
DELETE /api/candidates/me/cv

GET    /api/dashboard/candidate
```

### Recruiter

```text
GET    /api/jobs/my-jobs
GET    /api/jobs/{jobId}/applications
PATCH  /api/job-applications/{applicationId}/status

GET    /api/dashboard/recruiter
```

---

# Ownership & Security

The application does not trust `CandidateId` or `RecruiterId` supplied by the client.

Instead, ownership is resolved from the authenticated JWT identity.

```text
JWT UserId
    │
    ├── Candidate.UserId
    │       ↓
    │   Candidate.Id
    │       ↓
    │   JobApplication.CandidateId
    │
    └── Recruiter.UserId
            ↓
        Recruiter.Id
            ↓
        Job.RecruiterId
            ↓
           Job
```

This ensures:

* Candidates can only manage their own applications.
* Recruiters can only manage their own jobs.
* Recruiters can only manage applications belonging to their jobs.
* Users cannot impersonate another Candidate or Recruiter by submitting an ID.
* User-specific dashboard data is scoped to the authenticated user.

---

# Business Rules

## Applying

* Only authenticated candidates can apply.
* The job must be `Open`.
* A candidate cannot apply to the same job more than once.
* New applications start with `Applied`.
* Candidate identity is derived from the authenticated user.

## Updating

Candidates can update candidate-provided application information while the application is editable.

Candidates cannot modify:

```text
CandidateId
JobId
ApplicationId
Application Status
CreatedAt
```

## Cancellation

Candidates can cancel applications while they are in an eligible recruitment stage.

```text
Applied
UnderReview
Interview
```

Cancellation changes:

```text
Status = Cancelled
```

The application remains stored in the database.

## Recruiter Status Management

Recruiters can update application status for applications belonging to their jobs.

Typical workflow:

```text
Applied
   ↓
UnderReview
   ↓
Interview
   ↓
Accepted / Rejected
```

Recruiters cannot use the status endpoint to set an application to `Cancelled`.

---

# Architecture

The project follows **Clean Architecture** with a **CQRS/MediatR** application layer.

```text
┌───────────────────────────────┐
│       JobApplication.API      │
│                               │
│ Controllers / Middleware      │
└───────────────┬───────────────┘
                │
                ▼
┌───────────────────────────────┐
│   JobApplication.Application  │
│                               │
│ CQRS / MediatR                │
│ Commands / Queries / Handlers  │
│ DTOs / Interfaces             │
└───────────────┬───────────────┘
                │
                ▼
┌───────────────────────────────┐
│    JobApplication.Domain      │
│                               │
│ Entities / Enums / Rules      │
└───────────────┬───────────────┘
                │
                ▼
┌───────────────────────────────┐
│ JobApplication.Infrastructure │
│                               │
│ EF Core / Repositories        │
│ Unit of Work                  │
│ ASP.NET Identity              │
│ JWT / Refresh Tokens          │
│ Cloudinary                    │
│ Resend                        │
│ Hangfire                      │
└───────────────────────────────┘
```

---

# CQRS Structure

Features are organized by business capability.

```text
Application
└── Features
    ├── Auth
    │   ├── Commands
    │   │   ├── Register
    │   │   ├── Login
    │   │   ├── RefreshToken
    │   │   ├── Logout
    │   │   ├── RevokeRefreshToken
    │   │   ├── ChangePassword
    │   │   ├── ForgotPassword
    │   │   ├── ResetPassword
    │   │   ├── VerifyEmail
    │   │   └── ResendVerificationEmail
    │   │
    │   └── Queries
    │       └── GetCurrentUser
    │
    ├── Jobs
    ├── JobApplications
    ├── Interviews
    ├── Candidates
    └── Dashboard
```

The general request flow is:

```text
Controller
    ↓
MediatR
    ↓
Command / Query
    ↓
Handler
    ↓
Business Rules
    ↓
Repository / Specification
    ↓
UnitOfWork
    ↓
Database
```

---

# Data Architecture

The system uses separate databases for Identity and business data.

```text
             ┌─────────────────────┐
             │    Identity DB      │
             │                     │
             │ ApplicationUser     │
             │ ApplicationRole     │
             │ Refresh Tokens      │
             │ Identity Tables     │
             └──────────┬──────────┘
                        │
                        │ UserId
                        │
             ┌──────────▼──────────┐
             │    Business DB      │
             │                     │
             │ Candidate           │
             │ Recruiter           │
             │ Job                 │
             │ JobApplication      │
             │ Interview           │
             └─────────────────────┘
```

---

# Domain Relationships

```text
Candidate
    │
    │ 1
    │
    │ *
    ▼
JobApplication
    │
    │ *
    │
    │ 1
    ▼
Job
    │
    │ 1
    │
    │ *
    ▼
Interview
```

More precisely, an application can have multiple interviews:

```text
JobApplication
    │
    ├── Interview
    ├── Interview
    └── Interview
```

A candidate can submit multiple applications.

A job can receive multiple applications.

The same candidate cannot apply to the same job more than once.

---

# Infrastructure Integrations

## Cloudinary

Used for candidate CV storage.

```text
Candidate
    ↓
CV Upload
    ↓
Cloudinary
    ↓
CV Metadata in Business DB
```

## Resend

Used for transactional email:

```text
Email Verification
Password Reset
Verification Resend
```

## Hangfire

Used for background processing:

```text
Application
    ↓
Hangfire
    ↓
Background Job
    ↓
Notification / Email
```

---

# Design Patterns & Practices

The project uses or is designed around:

* Clean Architecture
* CQRS
* MediatR
* Repository Pattern
* Unit of Work
* Specification Pattern
* Dependency Injection
* DTOs
* Mapster
* Role-Based Authorization
* JWT Authentication
* Refresh Token Rotation / Revocation
* ASP.NET Core Identity
* Database-side aggregation
* Background Jobs
* External File Storage
* Transactional Email

---

# Technology Stack

* **.NET**
* **ASP.NET Core Web API**
* **C#**
* **Entity Framework Core**
* **SQL Server**
* **ASP.NET Core Identity**
* **JWT Authentication**
* **MediatR**
* **CQRS**
* **Clean Architecture**
* **Repository Pattern**
* **Unit of Work**
* **Specification Pattern**
* **Mapster**
* **Hangfire**
* **Cloudinary**
* **Resend**
* **Swagger / Scalar**
* **Git / GitHub**

---

# Project Goals

The project is designed to demonstrate how a real-world recruitment platform can be structured using modern .NET architecture and engineering practices.

The main focus is on:

* Secure authentication
* Clear separation of responsibilities
* Maintainable business logic
* CQRS-based application flow
* Strong ownership and authorization rules
* Recruitment workflow management
* Background processing
* External service integration
* Scalable read/query operations
* Clean and testable architecture
