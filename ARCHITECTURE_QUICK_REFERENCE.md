# KafkaWorkflow - Quick Architecture Reference

## 🎯 System Overview

A distributed .NET 10 application demonstrating event-driven architecture with Kafka, SQL Server, and Aspire orchestration.

**Core Components:**
- **WebAPI**: REST API for CRUD operations on People, Contacts, Addresses
- **Consumer**: Kafka-driven workflow processor with validation steps
- **DataAccess**: Entity Framework with SQL Server persistence
- **AppHost**: Aspire-based orchestration of all services

---

## 📊 High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   External Users/Tests                   │
└────────────────────┬──────────────────────────────────────┘
                     │ HTTP
                     ▼
┌─────────────────────────────────────────────────────────┐
│                    WebAPI Service                        │
│   (PeopleController, ContactController, AddressCtrl)    │
└────────┬──────────────────────────────────────────────────┘
         │ Database & Kafka Operations
         ├──────────────────────┬───────────────────────┐
         │                      │                       │
         ▼                      ▼                       ▼
    ┌─────────┐         ┌──────────────┐          ┌──────────┐
    │SQL Server│        │Kafka Broker  │          │DataAccess│
    │(Persist) │        │(Events)      │          │(EF Core) │
    └─────────┘         └──────┬───────┘          └──────────┘
                                │
                                │ Topics: people-topic
                                │
                                ▼
                        ┌──────────────────┐
                        │ Consumer Worker  │
                        │ (BackgroundSvc)  │
                        └────────┬─────────┘
                                 │
                                 ▼
                        ┌──────────────────┐
                        │PersonWorkflow    │
                        │+ Validation Steps│
                        └────────┬─────────┘
                                 │
                                 ▼
                        ┌──────────────────┐
                        │  Update State    │
                        │  in Database     │
                        └──────────────────┘
```

---

## 📦 Projects & Responsibilities

| Project | Type | Responsibility |
|---------|------|-----------------|
| **KafkaWorkflow.WebApi** | Web API | REST endpoints, HTTP handling, Kafka produce |
| **KafkaWorkflow.Consumer** | Worker | Background service, Kafka consume, workflow orchestration |
| **KafkaWorkflow.DataAccess** | Class Lib | EF Core DbContext, entities, database models |
| **KafkaWorkflow.AppHost** | Aspire Host | Container orchestration, service wiring |
| **KafkaWorkflow.ServiceDefaults** | Class Lib | Shared configuration, health checks, logging |
| **KafkaWorkflow.Test** | Unit Tests | Workflow & step tests with mocks |
| **KafkaWorkflow.PlaywrightTests** | E2E Tests | HTTP-based controller tests |
| **KafkaWorkflow.PlaywrightTests.TypeScript** | E2E Tests | TypeScript-based Playwright tests |

---

## 🔄 Request Flow Example: Create Person

```
1. HTTP POST /people {firstName, lastName, ...}
   │
   ▼
2. PeopleController.Post()
   │ • Add person to DbContext
   │ • SaveChangesAsync()
   │
   ├──────────────────┬─────────────────┐
   │                  │                  │
   ▼                  ▼                  ▼
3. Database      Kafka Producer   HTTP Response
   (Persist)      (Publish Event)   (201 Created)
                   │
                   ▼
4. Kafka Topic: people-topic
   {Key: "PersonId", Value: "Create"}
   │
   ▼
5. ConsumerWorker
   Kafka.Consumer.Subscribe("people-topic")
   │
   ▼
6. PersonWorkflow.OnExecuteAsync()
   │ • GetStateAsync() → Load from DB
   │ • Execute Steps:
   │   - ValidatePersonStep
   │   - ValidateContactStep
   │   - ValidateAddressStep
   │ • UpdateState()
   │
   ▼
7. Workflow Complete
   State persisted to database
```

---

## 🏗️ Workflow Architecture

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
│                                          │
│ • Steps Collection                       │
│   ├─ ValidatePersonStep                  │
│   ├─ ValidateContactStep                 │
│   └─ ValidateAddressStep                 │
│                                          │
└──────────────────────────────────────────┘
```

---

## 💾 Entity Relationships

```
Person (1) ──────── (N) ContactInfo (1) ──────── (N) Address
│                        │                            │
├─ Id (PK)              ├─ Id (PK)                 ├─ Id (PK)
├─ FirstName           ├─ PersonId (FK)           ├─ ContactInfoId (FK)
├─ LastName            ├─ Email                   ├─ Street
├─ DateOfBirth         ├─ Phone                   ├─ City
├─ CreatedAt           ├─ Type                    ├─ State
└─ UpdatedAt           └─ CreatedAt/UpdatedAt     └─ ZipCode/Country
```

---

## 🚀 Services & Ports

| Service | Port(s) | Technology | Purpose |
|---------|---------|-----------|---------|
| **webapi** | 5000/5001 | ASP.NET Core | REST API |
| **kafka** | 9092 | Apache Kafka | Message broker |
| **kafka-ui** | 8080 | Kafka UI | Kafka management |
| **database** | 57242 | SQL Server | Data storage |
| **kafka-consumer** | (background) | .NET Worker | Event processing |

---

## 🧪 Testing Layers

### Unit Tests
```
├─ PersonWorkflowTests
│  ├─ Mocks: IWorkflowLogger, IMessageWorkflowStep
│  ├─ Tests step execution, error handling
│  └─ Framework: NUnit + Moq
│
├─ ValidatePersonStepTests
├─ ValidateContactStepTests
└─ ValidateAddressStepTests
   └─ All test individual validation logic
```

### E2E Tests (C#)
```
├─ PeopleControllerTests
├─ ContactControllerTests
├─ AddressControllerTests
├─ MultiControllerWorkflowTests
└─ PeopleControllerErrorHandlingTests
   └─ All use PlaywrightFixture for HTTP + browser testing
```

### E2E Tests (TypeScript)
```
├─ people.spec.ts
├─ contact.spec.ts
└─ address.spec.ts
   └─ All use Playwright Test framework
```

---

## 🎛️ Aspire Orchestration

```
AppHost.cs (DistributedApplication)
│
├─ SQL Server Container
│  └─ "People" database + initSql.sql
│
├─ Kafka Container
│  └─ Kafka UI dashboard
│
├─ WebAPI Project
│  ├─ Depends: database, kafka
│  └─ Waits for: both
│
├─ Consumer Project
│  ├─ Depends: database, kafka
│  └─ Waits for: both
│
└─ Playwright Tests Project
   ├─ ExplicitStart: true (manual trigger)
   ├─ Depends: webapi
   └─ Waits for: webapi
```

---

## 🔐 Key Interfaces

```csharp
// Workflow Definition
public interface IMessageWorkflow<T, TState>
{
    IReadOnlyCollection<IMessageWorkflowStep<T, TState>> Steps { get; }
    IObjectAccessor<TState> StateAccessor { get; }
    IWorkflowLogger<T, TState> Logger { get; }
    Task OnExecuteAsync(T message, CancellationToken ct = default);
}

// Workflow Step
public interface IMessageWorkflowStep<T, TState>
{
    Task<bool> ShouldExecuteAsync();
    Task OnPreExecuteAsync(CancellationToken ct = default);
    Task ExecuteAsync(CancellationToken ct = default);
    Task OnCompleteAsync(CancellationToken ct = default);
    Task<bool> OnErrorAsync(Exception ex, CancellationToken ct = default);
}

// State Accessor
public interface IObjectAccessor<T>
{
    T? Value { get; set; }
}

// Workflow Logger
public interface IWorkflowLogger<T, TState>
{
    Task CollectAsync<TStep>(WorkflowStage stage, string message, 
        Exception? ex, CancellationToken ct = default);
    Task WriteAsync(CancellationToken ct = default);
}
```

---

## 📈 Data Flow Diagram

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
           │  └──────┬───────┘
           │         │
           │         ▼
           │  ┌──────────────────┐
           │  │ Consumer Worker  │
           │  │ (Async)          │
           │  └──────┬───────────┘
           │         │
           │         ▼
           │  ┌──────────────────┐
           │  │ Workflow Steps   │
           │  │ (Validation)     │
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

## 🛠️ Running the System

### Start All Services
```bash
cd src
dotnet run --project KafkaWorkflow.AppHost
# Opens Aspire Dashboard at http://localhost:15251
```

### Run Unit Tests
```bash
dotnet test test/KafkaWorkflow.Test
```

### Run E2E Tests (C#)
```bash
# Via Aspire Dashboard (recommended)
# OR standalone:
dotnet test test/KafkaWorkflow.PlaywrightTests
```

### Run E2E Tests (TypeScript)
```bash
cd test/KafkaWorkflow.PlaywrightTests.TypeScript
npm install
npx playwright test
```

### View Kafka Topics
```
Open browser: http://localhost:8080
(Kafka UI - provided by Aspire)
```

---

## 📚 Key Files Reference

| File | Purpose |
|------|---------|
| `AppHost.cs` | Service orchestration definition |
| `Program.cs` (WebAPI) | Dependency injection setup |
| `Program.cs` (Consumer) | Worker registration & configuration |
| `PeopleContext.cs` | EF Core DbContext definition |
| `BusinessWorkflow.cs` | Generic workflow template |
| `PersonWorkflow.cs` | Concrete workflow implementation |
| `PlaywrightFixture.cs` | Base class for E2E tests |
| `ARCHITECTURE.md` | Detailed architecture documentation |

---

## ✅ Architecture Principles

1. **Separation of Concerns** - Each project has single responsibility
2. **Dependency Inversion** - Depend on abstractions (interfaces)
3. **Async/Await** - Non-blocking I/O throughout
4. **Event-Driven** - Loose coupling via Kafka
5. **Testability** - Mockable dependencies, comprehensive test coverage
6. **Scalability** - Stateless services, consumer groups
7. **Observability** - Aspire dashboard, structured logging
8. **Cloud-Native** - Container-ready, infrastructure-as-code

---

## 🚀 Next Steps

1. **Review** `ARCHITECTURE.md` for detailed component documentation
2. **Explore** AppHost.cs to understand service wiring
3. **Run** `dotnet run --project KafkaWorkflow.AppHost`
4. **Test** via Aspire Dashboard or CLI commands above
5. **Extend** with additional workflows or services

---

**Technology:** .NET 10 | **Architecture:** Event-Driven Microservices | **Orchestration:** Aspire
