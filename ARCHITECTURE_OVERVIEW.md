# KafkaWorkflow - Architecture Overview

A distributed .NET 10 application demonstrating event-driven architecture with Kafka, SQL Server, and Aspire orchestration.

**Status**: Complete & Current | **Version**: 2.0 (Three-Topic Architecture) | **Last Updated**: 2024

---

## 📑 Table of Contents

1. [System Overview](#system-overview)
2. [High-Level Architecture](#high-level-architecture)
3. [Core Components](#core-components)
4. [Projects & Responsibilities](#projects--responsibilities)
5. [Architecture Principles](#architecture-principles)
6. [Request Flow Examples](#request-flow-examples)
7. [Workflow Architecture](#workflow-architecture)
8. [Entity Relationships](#entity-relationships)
9. [Data Flow Diagram](#data-flow-diagram)
10. [Services & Ports](#services--ports)
11. [Testing Layers](#testing-layers)
12. [Aspire Orchestration](#aspire-orchestration)
13. [Key Interfaces](#key-interfaces)
14. [Running the System](#running-the-system)
15. [Key Files Reference](#key-files-reference)

---

## System Overview

The KafkaWorkflow solution implements a **distributed event-driven microservices architecture** with:

- **Event-Driven Design**: Loose coupling via Apache Kafka
- **Workflow Orchestration**: Multi-step validation pipeline
- **Async Processing**: Non-blocking I/O throughout
- **Cloud-Native**: Aspire-based orchestration
- **Comprehensive Testing**: Unit, and E2E (C#) tests
- **Production-Ready**: Health checks, logging, error handling

**Technology Stack:**
- **.NET 10** - Core framework
- **ASP.NET Core** - REST API
- **Apache Kafka** - Event broker (3 topics)
- **SQL Server** - Data persistence
- **Entity Framework Core** - ORM
- **Aspire** - Container orchestration
- **Playwright** - E2E testing
- **NUnit + Moq** - Unit testing

---

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   External Users/Tests                  │
└────────────────────┬────────────────────────────────────┘
                     │ HTTP
                     ▼
┌─────────────────────────────────────────────────────────┐
│                    WebAPI Service                       │
│   (PeopleController, ContactController, AddressCtrl)    │
│                                                         │
│ Routes:                                                 │
│ • POST /people, POST /contact/{personId}                │
│ • POST /address/{contactInfoId}, + PUT/DELETE/GET       │
└────────┬────────────────────────────────────────────────┘
         │ Database & Kafka Operations
         ├──────────────────────┬───────────────────────┐
         │                      │                       │
         ▼                      ▼                       ▼
    ┌─────────┐         ┌──────────────┐          ┌──────────┐
    │SQL Server│        │Kafka Broker  │          │DataAccess│
    │(Persist) │        │(3 Topics)    │          │(EF Core) │
    └─────────┘         └──────┬───────┘          └──────────┘
                                │
                    ┌───────────┼───────────┐
                    │           │           │
                    ▼           ▼           ▼
            ┌──────────┐ ┌──────────┐ ┌──────────┐
            │people-   │ │contact-  │ │address-  │
            │topic     │ │topic     │ │topic     │
            └────┬─────┘ └────┬─────┘ └────┬─────┘
                 │            │            │
                 ▼            ▼            ▼
            ┌──────────┐ ┌──────────┐ ┌──────────┐
            │People    │ │Contact   │ │Address   │
            │Worker    │ │Worker    │ │Worker    │
            └────┬─────┘ └────┬─────┘ └────┬─────┘
                 │            │            │
                 └─────┬──────┴──────┬─────┘
                       │             │
                       ▼             ▼
                  ┌───────────────────────┐
                  │  PersonWorkflow       │
                  │  (Unified Orchestrator)
                  │  3-Step Validation    │
                  └──────────┬────────────┘
                             │
                             ▼
                  ┌───────────────────────┐
                  │  Update State         │
                  │  in Database          │
                  └───────────────────────┘
```

---

## Core Components

### 1. **WebAPI Service** (REST Endpoints)
Handles HTTP requests for CRUD operations on People, Contacts, and Addresses.

**Key Responsibilities:**
- Parse and validate HTTP requests
- Persist entities to SQL Server
- Publish events to Kafka
- Return HTTP responses with appropriate status codes

**Endpoints:**
- `GET /people` - List all people
- `GET /people/{id}` - Get person by ID
- `POST /people` - Create person
- `PUT /people` - Update person
- `DELETE /people/{id}` - Delete person
- `GET /contact` - List all contacts
- `GET /contact/{id}` - Get contact by ID
- `POST /contact/{personId}` - Create contact (route parameter)
- `PUT /contact` - Update contact
- `DELETE /contact/{id}` - Delete contact
- `GET /address` - List all addresses
- `GET /address/{id}` - Get address by ID
- `POST /address/{contactInfoId}` - Create address (route parameter)
- `PUT /address` - Update address
- `DELETE /address/{id}` - Delete address

### 2. **Kafka Broker** (Event Bus)
Three separate topics for event distribution:

| Topic | Purpose | Worker | Events |
|-------|---------|--------|--------|
| `people-topic` | Person events | PeopleWorker | Create, Update, Delete |
| `contact-topic` | ContactInfo events | ContactWorker | Create, Update, Delete |
| `address-topic` | Address events | AddressWorker | Create, Update, Delete |

**Consumer Group**: `people-consumer-group`  
**Replication Factor**: 1 per topic  
**Partitions**: 1 per topic

### 3. **Consumer Workers** (Event Processing)
Three background services that consume Kafka events:

- **PeopleWorker** - Subscribes to `people-topic`
- **ContactWorker** - Subscribes to `contact-topic`
- **AddressWorker** - Subscribes to `address-topic`

All workers execute the unified **PersonWorkflow** with three validation steps.

### 4. **PersonWorkflow** (Orchestration)
Generic workflow framework that:
- Loads entity state from database
- Executes validation steps sequentially
- Handles errors with retry logic
- Updates final state in database

**Validation Steps:**
1. **ValidatePersonStep** - Validates person data
2. **ValidateContactStep** - Validates contact associations
3. **ValidateAddressStep** - Validates address associations

### 5. **SQL Server** (Data Persistence)
Relational database storing:
- **Persons** - Person entities
- **ContactInfos** - Contact information
- **Addresses** - Address information

---

## Projects & Responsibilities

| Project | Type | Responsibility |
|---------|------|-----------------|
| **KafkaWorkflow.WebApi** | Web API | REST endpoints, HTTP handling, Kafka produce |
| **KafkaWorkflow.Consumer** | Worker Service | Background service, Kafka consume, workflow orchestration |
| **KafkaWorkflow.DataAccess** | Class Library | EF Core DbContext, entities, database models |
| **KafkaWorkflow.AppHost** | Aspire Host | Container orchestration, service wiring |
| **KafkaWorkflow.ServiceDefaults** | Class Library | Shared configuration, health checks, logging |
| **KafkaWorkflow.Test** | Unit Tests | Workflow & step tests with mocks |
| **KafkaWorkflow.PlaywrightTests** | E2E Tests | HTTP-based controller tests (C#) |

---

## Architecture Principles

1. **Separation of Concerns** - Each project has single responsibility
2. **Dependency Inversion** - Depend on abstractions (interfaces)
3. **Async/Await** - Non-blocking I/O throughout
4. **Event-Driven** - Loose coupling via Kafka
5. **Testability** - Mockable dependencies, comprehensive test coverage
6. **Scalability** - Stateless services, consumer groups
7. **Observability** - Aspire dashboard, structured logging
8. **Cloud-Native** - Container-ready, infrastructure-as-code

---

## Request Flow Examples

### Example 1: Create Person → Contact → Address Chain

```
STEP 1: POST /people
│
├─ Controller saves Person to database
├─ Kafka topic "people-topic" receives event
└─ HTTP 201 Created response sent to client
   │
   └──────────────────────┐
                          │
                          ▼ (Asynchronous Processing)
                  PeopleWorker consumes event
                  │
                  ▼
           PersonWorkflow.ExecuteAsync()
           │
           ├─ ValidatePersonStep: Validates firstName, lastName, DOB
           ├─ ValidateContactStep: Validates related contacts
           ├─ ValidateAddressStep: Validates related addresses
           │
           ▼
           Database state updated


STEP 2: POST /contact/{personId}
│
├─ Controller adds ContactInfo to Person
├─ Kafka topic "contact-topic" receives event
└─ HTTP 201 Created response sent to client
   │
   └──────────────────────┐
                          │
                          ▼
                  ContactWorker consumes event
                  │
                  ▼
           PersonWorkflow.ExecuteAsync()
           │ (Same 3-step validation)
           ▼
           Database state updated


STEP 3: POST /address/{contactInfoId}
│
├─ Controller adds Address to ContactInfo
├─ Kafka topic "address-topic" receives event
└─ HTTP 201 Created response sent to client
   │
   └──────────────────────┐
                          │
                          ▼
                  AddressWorker consumes event
                  │
                  ▼
           PersonWorkflow.ExecuteAsync()
           │ (Same 3-step validation)
           ▼
           Database state updated
```

### Example 2: Synchronous Response Path

```
1. Client sends HTTP POST /people with body
   │
   ▼
2. PeopleController.Post() executes
   ├─ Validate request
   ├─ Create Person entity
   ├─ Add to DbContext
   ├─ SaveChangesAsync() → Database insert
   │
   ▼
3. KafkaProducer.ProduceAsync() → Publish to people-topic
   │
   ▼
4. HTTP 201 Created response sent to client
   {
     "id": 123,
     "firstName": "Alice",
     "lastName": "Johnson",
     "dateOfBirth": "1990-03-15"
   }

(Meanwhile, asynchronously)
5. PeopleWorker consumes from people-topic
6. PersonWorkflow validates the entire entity chain
7. Database state is finalized
```

---

## Workflow Architecture

### Class Hierarchy

```
┌──────────────────────────────────────────┐
│ IMessageWorkflow<T, TState> (Interface)  │
└─────────────────────┬────────────────────┘
                      │ implements
                      ▼
┌──────────────────────────────────────────┐
│ BusinessWorkflow<T, TState> (Abstract)   │
│                                          │
│ • OnExecuteAsync()                       │
│   └─ For each step in Steps:             │
│      1. ShouldExecuteAsync()             │
│      2. OnPreExecuteAsync()              │
│      3. ExecuteAsync()                   │
│      4. OnCompleteAsync()                │
│      5. OnErrorAsync() if exception      │
│                                          │
│ • OnGetStateAsync() [ABSTRACT]           │
│                                          │
└─────────────────────┬────────────────────┘
                      │ extends
                      ▼
┌──────────────────────────────────────────┐
│ PersonWorkflow : BusinessWorkflow        │
│ <int, PersonState>                       │
│                                          │
│ • OnGetStateAsync()                      │
│   └─ Loads Person from database          │
│      with Include(ContactInfos)          │
│      .ThenInclude(Addresses)             │
│                                          │
│ • Steps Collection                       │
│   ├─ ValidatePersonStep                  │
│   ├─ ValidateContactStep                 │
│   └─ ValidateAddressStep                 │
│                                          │
└──────────────────────────────────────────┘
```

### Workflow Execution Pipeline

```
1. Message received from Kafka
   │
   ▼
2. Workflow.OnExecuteAsync()
   │
   ├─ GetStateAsync()
   │  └─ Load Person with related data
   │
   ├─ For each step in Steps:
   │  │
   │  ├─ ShouldExecuteAsync()
   │  │  └─ Check preconditions
   │  │
   │  ├─ OnPreExecuteAsync()
   │  │  └─ Initialize step
   │  │
   │  ├─ ExecuteAsync()
   │  │  └─ Run business logic
   │  │
   │  ├─ OnCompleteAsync()
   │  │  └─ Finalize step
   │  │
   │  └─ OnErrorAsync() [if exception]
   │     └─ Handle error, decide continue/stop
   │
   ├─ Logger.WriteAsync()
   │  └─ Persist logs
   │
   ▼
3. Workflow Complete
   └─ State persisted to database
```

---

## Entity Relationships

```
Person (1) ──────── (N) ContactInfo (1) ──────── (N) Address
│                        │                            │
├─ Id (PK)               ├─ Id (PK)                   ├─ Id (PK)
├─ FirstName             ├─ PersonId (FK)             ├─ ContactInfoId (FK)
├─ LastName              ├─ Email                     ├─ Street
├─ DateOfBirth           ├─ Phone                     ├─ City
├─ CreatedAt             ├─ Type                      ├─ State
└─ UpdatedAt             └─ CreatedAt/UpdatedAt       └─ ZipCode/Country

CASCADE DELETE: 
Person (delete) → ContactInfos (delete) → Addresses (delete)
```

---

## Data Flow Diagram

```
HTTP Request
    │
    ▼
┌─────────────────────┐
│ REST Controller     │
│ (Validate input)    │
└──────────┬──────────┘
           │
           ├─ Write to DB (async)
           │      │
           │      ▼
           │  ┌─────────────┐
           │  │ SQL Server  │
           │  │ (Persist)   │
           │  └─────────────┘
           │
           ├─ Publish to Kafka
           │      │
           │      ▼
           │  ┌──────────────┐
           │  │ Kafka Topic  │
           │  │ (3 topics)   │
           │  └──────┬───────┘
           │         │
           │         ▼
           │  ┌──────────────────┐
           │  │ Consumer Worker  │
           │  │ (PeopleWorker,   │
           │  │  ContactWorker,  │
           │  │  AddressWorker)  │
           │  └──────┬───────────┘
           │         │
           │         ▼
           │  ┌──────────────────┐
           │  │ PersonWorkflow   │
           │  │ (3 Validation    │
           │  │  Steps)          │
           │  └──────┬───────────┘
           │         │
           │         ▼
           │  ┌──────────────────┐
           │  │ Update DB        │
           │  │ (Final state)    │
           │  └──────────────────┘
           │
           ▼
    ┌─────────────────┐
    │ HTTP Response   │
    │ 201/200/4xx/5xx │
    └─────────────────┘
```

---

## Services & Ports

| Service | Port(s) | Technology | Purpose |
|---------|---------|-----------|---------|
| **webapi** | 5000/5001 | ASP.NET Core 10 | REST API |
| **kafka-consumer** | (background) | .NET Worker | Event processing |
| **kafka** | 9092 | Apache Kafka 3.x | Message broker |
| **kafka-ui** | 8080 | Kafka UI | Broker management |
| **database** | 55245 | SQL Server 2025 | Data storage |
| **Aspire Dashboard** | 15251 | Aspire | Orchestration & monitoring |

---

## Testing Layers

### Unit Tests (Fast & Isolated)

```
├─ PersonWorkflowTests
│  ├─ Mocks: IWorkflowLogger, IMessageWorkflowStep
│  ├─ Tests step execution, error handling
│  └─ Framework: NUnit + Moq
│
├─ ValidatePersonStepTests
│  └─ Tests Person data validation
│
├─ ValidateContactStepTests
│  └─ Tests ContactInfo association validation
│
└─ ValidateAddressStepTests
   └─ Tests Address association validation
```

### Integration Tests (Medium Speed)

```
├─ PeopleControllerTests
│  └─ Tests Person CRUD operations
│
├─ ContactControllerTests
│  └─ Tests Contact CRUD with foreign key validation
│
├─ AddressControllerTests
│  └─ Tests Address CRUD with foreign key validation
│
├─ MultiControllerWorkflowTests
│  └─ Tests complete end-to-end workflows
│
└─ PeopleControllerErrorHandlingTests
   └─ Tests error scenarios (400, 404, etc.)
```

Framework: **NUnit + Playwright (C#)**

---

## Aspire Orchestration

```
AppHost.cs (DistributedApplication)
│
├─ SQL Server Container
│  ├─ Image: mssql/server:2025-latest
│  ├─ Port: 55245
│  ├─ Volume: sqlserver-data
│  └─ Database: People
│      └─ Script: initSql.sql
│
├─ Kafka Container
│  ├─ Image: confluentinc/cp-kafka:7.x
│  ├─ Port: 9092
│  ├─ Volume: kafka-data
│  └─ Topics:
│      ├─ people-topic
│      ├─ contact-topic
│      └─ address-topic
│
├─ Kafka UI Container
│  ├─ Image: provectuslabs/kafka-ui:latest
│  └─ Port: 8080
│
├─ WebAPI Project
│  ├─ Ports: 5000 (HTTP), 5001 (HTTPS)
│  ├─ Depends: database, kafka
│  └─ Waits for: both
│
└─ Consumer Project
   ├─ Type: Background Service
   ├─ Depends: database, kafka
   └─ Waits for: both

Aspire Dashboard: http://localhost:15251
```

---

## Key Interfaces

### IMessageWorkflow<T, TState>

```csharp
public interface IMessageWorkflow<T, TState>
{
    IReadOnlyCollection<IMessageWorkflowStep<T, TState>> Steps { get; }
    IObjectAccessor<TState> StateAccessor { get; }
    IWorkflowLogger<T, TState> Logger { get; }
    Task OnExecuteAsync(T message, CancellationToken ct = default);
}
```

### IMessageWorkflowStep<T, TState>

```csharp
public interface IMessageWorkflowStep<T, TState>
{
    Task<bool> ShouldExecuteAsync();
    Task OnPreExecuteAsync(CancellationToken ct = default);
    Task ExecuteAsync(CancellationToken ct = default);
    Task OnCompleteAsync(CancellationToken ct = default);
    Task<bool> OnErrorAsync(Exception ex, CancellationToken ct = default);
}
```

### IObjectAccessor<T>

```csharp
public interface IObjectAccessor<T>
{
    T? Value { get; set; }
}
```

### IWorkflowLogger<T, TState>

```csharp
public interface IWorkflowLogger<T, TState>
{
    Task CollectAsync<TStep>(WorkflowStage stage, string message, 
        Exception? ex, CancellationToken ct = default) where TStep : IMessageWorkflowStep<T, TState>;
    Task WriteAsync(CancellationToken ct = default);
}
```

---

## Running the System

### Start All Services (Recommended)

```bash
cd src
dotnet run --project KafkaWorkflow.AppHost
# Opens Aspire Dashboard at http://localhost:15251
```

### Run Unit Tests

```bash
dotnet test test/KafkaWorkflow.Test
```

### Run E2E Tests (C# via Aspire)

```bash
# From Aspire Dashboard:
# 1. Start services (see above)
# 2. Tests run automatically or click Run button

# From command line:
dotnet test test/KafkaWorkflow.PlaywrightTests
```

### View Kafka Topics

```
Open browser: http://localhost:8080
Username: admin (default)
```

### Check Service Health

```bash
# WebAPI health check
curl https://localhost:7252/health --insecure

# Aspire Dashboard
http://localhost:15251
```

---

## Key Files Reference

| File | Purpose |
|------|---------|
| **AppHost.cs** | Service orchestration definition |
| **KafkaWorkflow.WebApi/Program.cs** | Dependency injection setup for WebAPI |
| **KafkaWorkflow.Consumer/Program.cs** | Worker registration & configuration |
| **PeopleContext.cs** | EF Core DbContext definition |
| **BusinessWorkflow.cs** | Generic workflow template implementation |
| **PersonWorkflow.cs** | Concrete workflow implementation |
| **PlaywrightFixture.cs** | Base class for E2E tests |
| **initSql.sql** | Database initialization script |

---

## Documentation Files

| File | Purpose |
|------|---------|
| **README.md** | Project overview and getting started |
| **ARCHITECTURE_OVERVIEW.md** | This file - architecture details |
| **ARCHITECTURE_DIAGRAMS.md** | 15 comprehensive diagrams |
| **test/KafkaWorkflow.PlaywrightTests/ASPIRE_SETUP.md** | Test setup and troubleshooting |
| **CrossControllerIntegration.http** | HTTP test file for manual testing |

---

## Quick Links

- **Visual Diagrams**: See [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md)
- **Getting Started**: See [README.md](./README.md)
- **Testing Setup**: See [test/KafkaWorkflow.PlaywrightTests/ASPIRE_SETUP.md](./test/KafkaWorkflow.PlaywrightTests/ASPIRE_SETUP.md)

---

## Summary

KafkaWorkflow demonstrates a production-ready, event-driven microservices architecture with:

✅ **Clean Architecture** - Separation of concerns, SOLID principles  
✅ **Async Processing** - Non-blocking I/O, efficient resource usage  
✅ **Comprehensive Testing** - Unit, integration, and E2E test coverage  
✅ **Scalability** - Stateless services, message-based communication  
✅ **Observability** - Aspire dashboard, structured logging  
✅ **Cloud-Native** - Container-ready, infrastructure-as-code  
✅ **Production-Ready** - Error handling, health checks, resilience  

**For detailed visual representations, see [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md)**

---

**Technology Stack**: .NET 10 | **Architecture**: Event-Driven Microservices | **Orchestration**: Aspire

**Getting Started**: `dotnet run --project KafkaWorkflow.AppHost`
