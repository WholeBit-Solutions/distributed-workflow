# Kafka Distributed Workflow

An Aspire orchestrated distributed system demonstrating business workflow implementation using Kafka message broker consumers. This solution showcases event-driven architecture with multi-step validation workflows for managing People, Contacts, and Addresses.

## Overview

This project implements a **distributed workflow pattern** where:
- **WebApi** receives REST requests to create/update/delete entities
- **Kafka** serves as the message broker for event distribution
- **Consumer** processes events through a multi-step workflow with validation
- **Database** stores the processed and validated data

The workflow orchestration handles complex business logic across multiple services without tight coupling.

## Project Structure

### Core Projects

#### KafkaWorkflow.WebApi
REST API service providing endpoints for managing entities.

**Controllers:**
- `PeopleController` - Manage Person entities
  - `GET /people` - Get all people
  - `GET /people/{id}` - Get person by ID
  - `POST /people` - Create new person
  - `PUT /people` - Update person
  - `DELETE /people/{id}` - Delete person

- `ContactController` - Manage ContactInfo entities (associated with Person)
  - `GET /contact` - Get all contacts
  - `GET /contact/{id}` - Get contact by ID
  - `POST /contact?personId={id}` - Create contact for person
  - `PUT /contact` - Update contact
  - `DELETE /contact/{id}` - Delete contact

- `AddressController` - Manage Address entities (associated with ContactInfo)
  - `GET /address` - Get all addresses
  - `GET /address/{id}` - Get address by ID
  - `POST /address?contactInfoId={id}` - Create address for contact
  - `PUT /address` - Update address
  - `DELETE /address/{id}` - Delete address

#### KafkaWorkflow.Consumer
Kafka consumer service that processes events through multi-step workflows.

**Key Components:**
- `PeopleWorker` - Consumes person events
- `ContactWorker` - Consumes contact events
- `AddressWorker` - Consumes address events

**Workflow System:**
- `PersonWorkflow` - Orchestrates validation steps
- `ValidatePersonStep` - Validates person data
- `ValidateContactStep` - Validates contact associations
- `ValidateAddressStep` - Validates address associations

#### KafkaWorkflow.DataAccess
Data access layer with entity definitions and database context.

**Entities:**
- `Person` - Core entity representing a person
- `ContactInfo` - Contact information for a person (1-to-many)
- `Address` - Address information for a contact (1-to-many)

**Database:**
- `PeopleContext` - EF Core DbContext for SQL Server

#### KafkaWorkflow.ServiceDefaults
Shared configuration and utilities for all services.

**Features:**
- Kafka serialization/deserialization
- OpenTelemetry instrumentation
- Service discovery configuration
- Health checks setup

#### KafkaWorkflow.AppHost
Aspire orchestration project for local development.

**Resources:**
- SQL Server database container
- Kafka message broker container
- Kafka UI for monitoring
- WebApi service
- Consumer service

### Supporting Projects

#### KafkaWorkflow.Test
Unit and integration tests for the consumer workflow and database operations.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        HTTP Clients                         │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
                    ┌──────────────┐
                    │  WebApi      │
                    │  Controllers │
                    └──────┬───────┘
                           │
                           ▼
                    ┌──────────────┐
                    │    Kafka     │
                    │   Broker     │
                    └──────┬───────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
        ▼                  ▼                  ▼
   ┌─────────┐        ┌─────────┐        ┌─────────┐
   │ People  │        │ Contact │        │ Address │
   │ Worker  │        │ Worker  │        │ Worker  │
   └────┬────┘        └────┬────┘        └────┬────┘
        │                  │                  │
        └──────────────────┼──────────────────┘
                           │
                           ▼
                    ┌───────────────┐
                    │  Workflows &  │
                    │  Validators   │
                    └──────┬────────┘
                           │
                           ▼
                    ┌──────────────┐
                    │  SQL Server  │
                    │   Database   │
                    └──────────────┘
```

## Workflow Execution Flow

1. **Request** - Client sends HTTP request to WebApi
2. **Persist** - Entity is saved to database immediately
3. **Publish** - Kafka message is published with entity data
4. **Consume** - Consumer receives message from topic
5. **Validate** - Multi-step workflow validates entity relationships:
   - Step 1: Validate Person data
   - Step 2: Validate ContactInfo associations
   - Step 3: Validate Address associations
6. **Store State** - Workflow state is maintained throughout execution

## Entity Relationships

```
Person (1)
  ├── ContactInfo (Many)
  │     ├── Email (required)
  │     ├── Phone (optional)
  │     └── Address (Many)
  │           ├── Street (required)
  │           ├── City (required)
  │           ├── State (required)
  │           ├── PostalCode (required)
  │           └── Country (optional)
```

**Cascade Behavior:**
- Deleting Person → Deletes all ContactInfos → Deletes all Addresses
- Deleting ContactInfo → Deletes all Addresses

## Technology Stack

- **Framework**: .NET 10
- **Language**: C# 14.0
- **Message Broker**: Apache Kafka
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **API Documentation**: Scalar
- **Testing**: NUnit, Moq
- **Container Orchestration**: Aspire

## Getting Started

### Prerequisites
- .NET 10 SDK
- Docker (for Kafka and SQL Server)
- Visual Studio 2022+ or VS Code

### Running Locally

1. **Start the Aspire Host:**
   ```powershell
   dotnet run --project KafkaWorkflow.AppHost
   ```
   This starts:
   - SQL Server on localhost:55245
   - Kafka on localhost:9092
   - WebApi on https://localhost:7252
   - Consumer service (background)

2. **Access the API:**
   - Scalar UI: https://localhost:7252/scalar/v1
   - Health Check: https://localhost:7252/health

3. **Test the Workflow:**
   - Use the included `CrossControllerIntegration.http` file in VS Code REST Client
   - Or use Scalar UI in the browser

### Running Tests

```powershell
dotnet test test/KafkaWorkflow.Test/KafkaWorkflow.Test.csproj
```

## Testing Workflow

The `CrossControllerIntegration.http` file contains comprehensive test scenarios:
- Basic CRUD operations
- Multi-entity relationships
- Cascading updates
- Cascading deletes
- Error handling
- Employee onboarding workflow
- Complex query scenarios

## Key Features

✅ **Event-Driven Architecture** - Loosely coupled services using Kafka
✅ **Multi-Step Workflows** - Extensible workflow system with validation steps
✅ **Relationship Management** - Complex entity relationships with cascade behaviors
✅ **Async Processing** - Non-blocking message consumption and processing
✅ **Error Handling** - Comprehensive error scenarios and validations
✅ **Service Discovery** - Automatic service endpoint resolution
✅ **Health Checks** - Built-in health monitoring
✅ **OpenTelemetry** - Distributed tracing support

## Development Notes

### Adding New Validation Steps
1. Create new class inheriting from `MessageWorkflowStep<int, PersonState>`
2. Implement `ExecuteAsync` method
3. Register in `PersonWorkflow.OnExecuteAsync`

### Extending Entity Models
1. Modify entity in `KafkaWorkflow.DataAccess\Entities`
2. Add corresponding validation step if needed
3. Update database migrations

### Monitoring
- Kafka UI available at the Aspire dashboard
- SQL Server can be accessed via Azure Data Studio
- OpenTelemetry metrics for performance tracking

## License

This project is part of WholeBit Solutions' distributed workflow demonstration suite.
