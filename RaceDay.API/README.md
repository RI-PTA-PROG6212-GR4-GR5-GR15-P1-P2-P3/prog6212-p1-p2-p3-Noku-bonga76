# RaceDay API — Part 2

**Module:** PROG6212 — Programming 2B  
**Student:** Nokubonga Mahlangu  
**Student number:** ST10484080  
**Project:** RaceDay RESTful API

## 1. Project Overview

RaceDay is an event-management system designed to support running events. Part 2 develops the backend RESTful API using C# and ASP.NET Core Web API. The API connects to Microsoft SQL Server using Entity Framework Core and provides endpoints for working with users, events, categories, enrolments, results and event images.

The API follows the requirements and planning produced for the earlier part of the assignment. Swagger/OpenAPI is used to inspect and test the API endpoints.

## User Roles

### Participant
A Participant can use the functions permitted to participants, such as viewing events and enrolling in event categories, subject to the implemented API permissions.

### Organiser
An Organiser can use the functions permitted to organisers, such as creating and managing events and their related information, subject to the implemented API permissions.

Authentication, session handling and role restrictions must be verified against the final source code and tests.

## Technologies

- C#
- ASP.NET Core Web API
- .NET 8 target framework
- Entity Framework Core
- Microsoft SQL Server
- Swagger / OpenAPI
- xUnit for unit testing, if included in the final test project
- GitHub Actions for continuous integration
.

##  Database

The API uses a SQL Server database named `RaceDay`. The database contains the following main entities:

- Users
- Events
- Categories
- Enrolments
- Results
- EventImages

The API uses Entity Framework Core for database access. The database schema and SQL script should correspond to the approved ERD and endpoint plan.

## Running the API

### Prerequisites

- A compatible .NET SDK
- Microsoft SQL Server
- Visual Studio or the .NET CLI

### Steps

1. Open `RaceDay.API/RaceDay.API.csproj` or the solution in Visual Studio.
2. Configure `RaceDayConnection` for the local SQL Server instance.
3. Ensure that the database schema is available.
4. Open a terminal in the API project directory.
5. Run:

```bash
dotnet restore
dotnet build
dotnet run
```

6. Open the Swagger URL displayed in the terminal.
7. Test the available endpoints using appropriate request bodies and valid test data.

The local HTTPS port may differ between machines.

## API Testing

The test project should cover important behaviours, including:

- Authentication and registration validation
- Event management
- Role-based access restrictions
- Enrolment creation and retrieval
- Invalid references and duplicate scenarios
- Appropriate HTTP success and error responses

Run the tests from the repository root:

```bash
dotnet test RaceDay.API.Tests/RaceDay.API.Tests.csproj
```

The test command and path must match the actual project structure. Test results should be verified locally before submission.

### Successful Build Evidence

## Video Presentation

**YouTube presentation link:** https://youtu.be/VX6tYe2twNE 
https://youtu.be/VX6tYe2twNE

