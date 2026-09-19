# Job Application Management

The **Job Application Management** module allows candidates to apply for jobs and recruiters to manage applications submitted to their jobs.

It is built using **ASP.NET Core, Clean Architecture, Entity Framework Core, SQL Server, ASP.NET Core Identity, and JWT Authentication**.

## Features

### Candidate

* Apply for an open job
* View submitted applications
* View application details
* Update application information while it is still editable
* Cancel an application
* Track application status

### Recruiter

* View applications submitted to their jobs
* View individual application details
* Update application status
* Manage the recruitment process for their job applications

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

## Application Status

| Status        | Description                               |
| ------------- | ----------------------------------------- |
| `Applied`     | Candidate has submitted the application   |
| `UnderReview` | Recruiter is reviewing the application    |
| `Interview`   | Candidate has reached the interview stage |
| `Accepted`    | Candidate has been accepted               |
| `Rejected`    | Application has been rejected             |
| `Cancelled`   | Candidate cancelled the application       |

## API Endpoints

### Candidate Endpoints

#### Apply for a Job

```http
POST /api/jobs/{jobId}/applications
```

Creates a new application for an open job.

Example request:

```json
{
  "coverLetter": "I am interested in this position...",
  "resumeUrl": "https://example.com/resume.pdf",
  "yearsOfExperience": 1
}
```

`CandidateId` and `Status` are not provided by the client.

They are determined by the authenticated user and the application business rules.

---

#### Get My Applications

```http
GET /api/job-applications/my-applications
```

Returns applications submitted by the authenticated candidate.

---

#### Get Application

```http
GET /api/job-applications/{applicationId}
```

Allows an authorized candidate or recruiter to view an application.

---

#### Update Application

```http
PUT /api/job-applications/{applicationId}
```

Updates candidate-provided application information while the application is editable.

Example:

```json
{
  "coverLetter": "Updated cover letter...",
  "resumeUrl": "https://example.com/new-resume.pdf",
  "yearsOfExperience": 2
}
```

---

#### Cancel Application

```http
PATCH /api/job-applications/{applicationId}/cancel
```

Changes the application status to:

```text
Cancelled
```

The application is not physically deleted.

---

### Recruiter Endpoints

#### Get Applications for a Job

```http
GET /api/jobs/{jobId}/applications
```

Returns applications submitted for a job owned by the authenticated recruiter.

---

#### Update Application Status

```http
PATCH /api/job-applications/{applicationId}/status
```

Example:

```json
{
  "status": "UnderReview"
}
```

The recruiter can manage the application's recruitment status.

## Authorization

The API uses **JWT Authentication** and role-based authorization.

### Candidate

```text
POST   /api/jobs/{jobId}/applications
GET    /api/job-applications/my-applications
GET    /api/job-applications/{applicationId}
PUT    /api/job-applications/{applicationId}
PATCH  /api/job-applications/{applicationId}/cancel
```

### Recruiter

```text
GET    /api/jobs/{jobId}/applications
GET    /api/job-applications/{applicationId}
PATCH  /api/job-applications/{applicationId}/status
```

## Ownership & Security

The application does not trust `CandidateId` or `RecruiterId` from the client.

Instead:

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
```

This ensures that:

* Candidates can only manage their own applications.
* Recruiters can only manage applications for their own jobs.
* Users cannot submit another user's ID to access or modify data.

## Business Rules

### Applying

* Only authenticated candidates can apply.
* The job must be `Open`.
* A candidate cannot apply to the same job more than once.
* New applications start with `Applied`.
* Candidate ID is derived from the authenticated user.

### Updating

Candidates can update:

* Cover Letter
* Resume URL
* Expected Salary
* Years of Experience

Candidates cannot modify:

* Candidate ID
* Job ID
* Application Status
* Application ID
* Created At

### Cancellation

Candidates can cancel applications while they are in an eligible recruitment stage.

Cancellation changes:

```text
Status = Cancelled
```

The record remains in the database to preserve application history.

### Recruiter Status Management

Recruiters can update the status of applications belonging to their jobs.

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

## Architecture

The feature follows the existing Clean Architecture structure:

```text
JobApplication.Application
│
├── DTOs
│   └── JobApplications
│       ├── CreateJobApplicationRequest
│       ├── UpdateJobApplicationRequest
│       ├── UpdateApplicationStatusRequest
│       └── JobApplicationResponse
│
├── Interfaces
│   └── IJobApplicationService
│
└── Implementations
    └── JobApplicationService
```

API:

```text
JobApplication.API
└── Controllers
    └── JobApplicationsController
```

## Domain Relationship

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
```

A candidate can submit multiple applications.

A job can receive multiple applications.

The same candidate cannot apply to the same job more than once.

## Technology Stack

* **.NET**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **SQL Server**
* **ASP.NET Core Identity**
* **JWT Authentication**
* **Clean Architecture**
* **Repository Pattern**
* **Unit of Work**
* **Swagger / Scalar API Documentation**
