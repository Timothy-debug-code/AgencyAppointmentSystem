# Overview System

A .NET Web API solution for agencies to book appointments for customers and issue tokens. The system allows agencies to view and manage a queue of customers with appointments for a specific day.


## Features

- Customer appointment booking

- Token generation for appointments

- Daily appointment queue management

- User authentication and authorization

- API documentation with Swagger

## Technology Stack

- ASP.NET Core Web API (.NET 9.0)
  
- Entity Framework Core for data access
  
- SQL Server for the database
  
- Azure App Service for hosting
  
- NuGet for package management
  
- xUnit for unit testing
  
- Swagger/OpenAPI for API documentation
  
- AutoMapper for object mapping
  
- Dependency Injection pattern with built-in .NET DI container

## Architecture

This project implements a clean architecture with clear separation of concerns:

- Web Layer: Controllers, API endpoints, request/response models
  
- Business Layer: Services, business logic, domain models
  
- Data Layer: Repositories, database context, entity models

All components are designed with IoC/DI in mind for better testability and maintainability.

## Setup Instructions

### Prerequisites

- Visual Studio 2022 or later
  
- .NET 6.0 SDK or later (recommended 9.0)
  
- SQL Server (local or Azure)
  
- Git

### Installation

1. Clone the repository

https://github.com/Timothy-debug-code/AgencyAppointmentSystem

2. Open the solution in Visual Studio
   
cd agency-appointment-system

AgencyAppointmentSystem.sln

4. Update the connection string in appsettings.json to point to your SQL Server instance
   
Run database migrations

Update-Database

6. Build and run the application
   
dotnet build

dotnet run --project AgencyAppointmentSystem.Api

5.Access the Swagger documentation at:

https://localhost:5003/swagger


## API Documentation

The API is fully documented using Swagger. When running the application locally, navigate to /swagger to view and test all available endpoints.

Key endpoints include:

- POST /api/Customers - Create a customer

- POST /api/appointments - Create an appointment for the customer

- PUT /api/Appointments/{id}/status - Confirm the appointment

- POST /api/Tokens/issue/{appointmentId} - Issue a token for the appointment

- GET /api/Appointments/date/{date} - View the list of appointments for a specific date

- POST /api/tokens - Generate a token for an appointment

## Testing

The solution includes comprehensive unit tests using xUnit. To run the tests:

dotnet test

## Deployment

The API is configured for deployment to Azure App Service. The deployment workflow is automated through GitHub Actions.

## Contact

Timothy - aurelius.timothy.tomason@gmail.com

Project Link: https://github.com/Timothy-debug-code/AgencyAppointmentSystem

Link Azure: https://agency-appointment-api-gnarccaufkfwdwed.indonesiacentral-01.azurewebsites.net/swagger/index.html
