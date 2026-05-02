# KafkaWorkflow - Visual Architecture Diagrams

## System Context Diagram

```
┌────────────────────────────────────────────────────────────────┐
│                                                                 │
│                         Users / Test Clients                    │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐    │
│  │ Web Browser  │  │ HTTP Client  │  │ Playwright Tests │    │
│  │ (UI/API Docs)│  │ (REST Calls) │  │ (Automated E2E)  │    │
│  └──────┬───────┘  └──────┬───────┘  └────────┬─────────┘    │
│         │                 │                    │               │
│         └─────────────────┼────────────────────┘               │
│                           │                                     │
│                           ▼ HTTP(S)                             │
│        ┌──────────────────────────────────────┐                │
│        │                                      │                │
│        │  KafkaWorkflow System                │                │
│        │  (Aspire Orchestrated)               │                │
│        │                                      │                │
│        │  ┌─────────────────────────────────┐ │                │
│        │  │  WebAPI Service                 │ │                │
│        │  │  - Controllers                  │ │                │
│        │  │  - Kafka Producer               │ │                │
│        │  └────────┬──────────┬─────────────┘ │                │
│        │           │          │                │                │
│        │      Query│ Produce  │Persist        │                │
│        │           │          │                │                │
│        │    ┌──────▼─┐ ┌──────▼────┐         │                │
│        │    │ Events │ │ SQL Server │        │                │
│        │    │ (Kafka)│ │ (Database) │        │                │
│        │    └────┬───┘ └────────────┘        │                │
│        │         │                            │                │
│        │         ▼                            │                │
│        │  ┌──────────────────────────────┐  │                │
│        │  │  Consumer Service            │  │                │
│        │  │  - Workflow Processor        │  │                │
│        │  │  - Validation Steps          │  │                │
│        │  │  - Kafka Consumer            │  │                │
│        │  └────────┬─────────────────────┘  │                │
│        │           │                         │                │
│        │      Persist                        │                │
│        │           │                         │                │
│        │           ▼                         │                │
│        │    ┌──────────────┐                 │                │
│        │    │ SQL Server   │                 │                │
│        │    │ (Database)   │                 │                │
│        │    └──────────────┘                 │                │
│        │                                      │                │
│        └──────────────────────────────────────┘                │
│                                                                 │
│  Aspire Dashboard: http://localhost:15251                       │
│  Kafka UI: http://localhost:8080                                │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

---

## Component Interaction Diagram

```
┌──────────────────────────────────────────────────────────────────────┐
│                         SYNCHRONOUS PATH (HTTP)                       │
│                                                                        │
│  Request                                                              │
│    │                                                                  │
│    ▼                                                                  │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ Controller Layer                                               │ │
│  │ • PeopleController.Post(person)                               │ │
│  │ • PeopleController.Get()                                      │ │
│  │ • PeopleController.Put(person)                                │ │
│  │ • PeopleController.Delete(id)                                 │ │
│  │                                                                │ │
│  │ Responsibility:                                                │ │
│  │ ✓ Parse HTTP request                                          │ │
│  │ ✓ Validate input                                              │ │
│  │ ✓ Call business logic                                         │ │
│  │ ✓ Return HTTP response                                        │ │
│  └───────┬──────────────────────────────────┬────────────────────┘ │
│          │                                  │                      │
│          ├─ Write/Query                     │                      │
│          │                                  │                      │
│          ▼                                  ▼                      │
│    ┌────────────────────┐           ┌──────────────────┐          │
│    │ DbContext          │           │ Kafka Producer   │          │
│    │ (EF Core)          │           │                  │          │
│    │                    │           │ • ProduceAsync() │          │
│    │ Add/Update/Remove  │           │                  │          │
│    │ SaveChangesAsync() │           └────────┬─────────┘          │
│    └────────┬───────────┘                    │                   │
│             │                                │                    │
│             └───────────────┬────────────────┘                   │
│                             │                                    │
│                             ▼                                    │
│                     ┌──────────────┐                             │
│                     │ SQL Server   │                             │
│                     │ Database     │                             │
│                     │              │                             │
│                     │ • Persists   │                             │
│                     │ • Returns ID │                             │
│                     └──────────────┘                             │
│                                                                   │
│  Response ◄────────────────────────────────────────────────────  │
│  201 Created {id, ...}                                            │
│                                                                   │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│                      ASYNCHRONOUS PATH (EVENT-DRIVEN)                 │
│                                                                        │
│  Event Published                                                      │
│    │                                                                  │
│    ▼                                                                  │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ Kafka Topic: people-topic                                      │ │
│  │ Message: {Key: "123", Value: "Create"}                         │ │
│  │                                                                │ │
│  │ Responsibility:                                                │ │
│  │ ✓ Event persistence                                            │ │
│  │ ✓ Consumer group coordination                                  │ │
│  │ ✓ Message ordering (single partition)                         │ │
│  └────────────────┬─────────────────────────────────────────────┘ │
│                   │                                                 │
│                   ▼                                                 │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ Consumer Worker (BackgroundService)                            │ │
│  │                                                                │ │
│  │ • ListensOn: people-topic                                      │ │
│  │ • ConsumerGroup: "people-consumer-group"                       │ │
│  │ • When message received:                                       │ │
│  │   └─ Create PersonWorkflow instance                           │ │
│  │   └─ Call OnExecuteAsync(messageValue)                        │ │
│  └────────────┬────────────────────────────────────────────────┘ │
│               │                                                    │
│               ▼                                                    │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ PersonWorkflow : BusinessWorkflow<int, PersonState>           │ │
│  │                                                                │ │
│  │ OnExecuteAsync(personId) {                                     │ │
│  │   1. state = OnGetStateAsync(personId)                        │ │
│  │      └─ Query DB for person data                              │ │
│  │   2. foreach step in Steps {                                  │ │
│  │      - ShouldExecuteAsync()                                   │ │
│  │      - OnPreExecuteAsync()                                    │ │
│  │      - ExecuteAsync()                                         │ │
│  │      - OnCompleteAsync()                                      │ │
│  │      - Catch exceptions with OnErrorAsync()                   │ │
│  │   }                                                            │ │
│  │ }                                                              │ │
│  └────────────┬───────────────────────────────────────────────┘ │
│               │                                                  │
│               ├─────────────────────┬──────────────┐            │
│               │                     │              │            │
│               ▼                     ▼              ▼            │
│  ┌──────────────────┐  ┌──────────────────┐ ┌────────────┐  │
│  │ValidatePersonStep│  │ValidateContactStp│ │ValidateAdrs│  │
│  │                  │  │                  │ │Step        │  │
│  │ Validates:       │  │ Validates:       │ │Validates:  │  │
│  │ • FirstName      │  │ • Email          │ │• Street    │  │
│  │ • LastName       │  │ • Phone          │ │• City      │  │
│  │ • DateOfBirth    │  │ • ContactType    │ │• State     │  │
│  │                  │  │                  │ │• ZipCode   │  │
│  └────────────┬─────┘  └────────┬─────────┘ └──────┬─────┘  │
│               │                 │                   │        │
│               └─────────────────┼───────────────────┘        │
│                                 │                            │
│                                 ▼                            │
│                    ┌──────────────────────┐                  │
│                    │ Update DB            │                  │
│                    │ (Workflow State)     │                  │
│                    │                      │                  │
│                    │ • All validations    │                  │
│                    │   passed             │                  │
│                    │ • Mark as processed  │                  │
│                    │                      │                  │
│                    └──────────────────────┘                  │
│                                                              │
│  Workflow Complete ✓                                        │
│  (Asynchronously processed, no blocking)                   │
│                                                              │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Dependency Graph

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│  KafkaWorkflow.WebApi                                              │
│  ├─ Depends on:                                                    │
│  │  ├─ KafkaWorkflow.DataAccess (DbContext, Entities)             │
│  │  ├─ KafkaWorkflow.ServiceDefaults (Config, Logging)           │
│  │  ├─ Confluent.Kafka (Producer)                                │
│  │  └─ Microsoft.AspNetCore (Framework)                          │
│  │                                                                 │
│  └─ Exports: IProducer<string, string>                            │
│             HttpClient instances                                  │
│             REST endpoints                                        │
│                                                                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  KafkaWorkflow.Consumer                                            │
│  ├─ Depends on:                                                    │
│  │  ├─ KafkaWorkflow.DataAccess (DbContext, Entities)             │
│  │  ├─ KafkaWorkflow.ServiceDefaults (Config, Logging)           │
│  │  ├─ Confluent.Kafka (Consumer)                                │
│  │  └─ Microsoft.Extensions.* (DI, Logging)                      │
│  │                                                                 │
│  └─ Exports: ConsumerWorker (BackgroundService)                   │
│             PersonWorkflow (Workflow<T, TState>)                  │
│             Validation Steps                                      │
│                                                                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  KafkaWorkflow.DataAccess                                          │
│  ├─ Depends on:                                                    │
│  │  └─ EntityFrameworkCore (SQL Server)                           │
│  │                                                                 │
│  └─ Exports: PeopleContext (DbContext)                            │
│             Entity classes                                        │
│             DbSet<T> collections                                  │
│                                                                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  KafkaWorkflow.ServiceDefaults                                     │
│  ├─ Depends on:                                                    │
│  │  ├─ Microsoft.Extensions (DI, Config)                          │
│  │  └─ AspNetCore.Diagnostics (Health checks)                     │
│  │                                                                 │
│  └─ Exports: Extension methods for service registration           │
│             Health check builders                                 │
│             Logging configuration                                 │
│                                                                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  KafkaWorkflow.AppHost                                             │
│  ├─ Depends on:                                                    │
│  │  ├─ Aspire.Hosting (DistributedApplication)                    │
│  │  ├─ Confluent.Kafka (Admin client for topic creation)         │
│  │  └─ All project references (WebApi, Consumer, etc)             │
│  │                                                                 │
│  └─ Exports: Complete application orchestration                   │
│             Container definitions                                 │
│             Service dependencies                                  │
│                                                                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  KafkaWorkflow.Test                                                │
│  ├─ Depends on:                                                    │
│  │  ├─ KafkaWorkflow.Consumer (Workflows, Steps)                  │
│  │  ├─ NUnit (Test framework)                                     │
│  │  └─ Moq (Mocking library)                                      │
│  │                                                                 │
│  └─ Exports: Unit tests for workflows and steps                   │
│                                                                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  KafkaWorkflow.PlaywrightTests                                     │
│  ├─ Depends on:                                                    │
│  │  ├─ KafkaWorkflow.AppHost (Aspire orchestration)              │
│  │  ├─ Microsoft.Playwright (Browser automation)                  │
│  │  ├─ Aspire.Hosting.Testing (Test builder)                     │
│  │  └─ NUnit (Test framework)                                     │
│  │                                                                 │
│  └─ Exports: End-to-end HTTP tests                               │
│             Playwright browser tests                              │
│             Integration tests                                     │
│                                                                   │
└─────────────────────────────────────────────────────────────────────┘

External Dependencies:
├─ SQL Server (Database)
├─ Apache Kafka (Message Broker)
├─ Entity Framework Core (ORM)
├─ Confluent Kafka Client (.NET)
├─ Playwright (Browser Testing)
├─ Serilog (Logging)
└─ .NET 10 Runtime
```

---

## Workflow Execution Sequence

```
Start PersonWorkflow.OnExecuteAsync(personId: 1)
│
├─ 1️⃣ GetState
│  │   PersonWorkflow.OnGetStateAsync(1)
│  │   ├─ Query: Person.Find(1)
│  │   ├─ Load: ContactInfos (navigation)
│  │   ├─ Load: Addresses (navigation)
│  │   └─ Return: PersonState with all data
│  │
│  └──────────────────────────────────────────────┐
│                                                  │
├─ 2️⃣ ForEach Step in Steps (ordered execution)  │
│  │                                              │
│  ├─ Step 1: ValidatePersonStep                 │
│  │  ├─ ShouldExecuteAsync()                    │
│  │  │  └─ Check if person data needs validation
│  │  │
│  │  ├─ OnPreExecuteAsync()                     │
│  │  │  └─ Setup validation context             │
│  │  │                                           │
│  │  ├─ ExecuteAsync()                          │
│  │  │  ├─ Validate FirstName (not null/empty) │
│  │  │  ├─ Validate LastName (not null/empty)  │
│  │  │  ├─ Validate DateOfBirth (if present)   │
│  │  │  └─ Throw ValidationException if invalid
│  │  │                                           │
│  │  └─ OnCompleteAsync()                       │
│  │     └─ Cleanup validation context           │
│  │                                              │
│  │     💡 Exception occurred?                   │
│  │        └─ OnErrorAsync(ex)                  │
│  │           ├─ Log error                      │
│  │           └─ Return shouldContinue flag     │
│  │              ├─ true: Continue with next step
│  │              └─ false: Stop workflow        │
│  │                                              │
│  ├─ Step 2: ValidateContactStep                │
│  │  ├─ ShouldExecuteAsync()                    │
│  │  │  └─ Check if contact info exists         │
│  │  │                                           │
│  │  ├─ OnPreExecuteAsync()                     │
│  │  │  └─ Setup validation context             │
│  │  │                                           │
│  │  ├─ ExecuteAsync()                          │
│  │  │  ├─ Validate Email (format)              │
│  │  │  ├─ Validate Phone (format)              │
│  │  │  └─ Throw ValidationException if invalid
│  │  │                                           │
│  │  └─ OnCompleteAsync()                       │
│  │     └─ Cleanup validation context           │
│  │                                              │
│  └─ Step 3: ValidateAddressStep                │
│     ├─ ShouldExecuteAsync()                    │
│     │  └─ Check if address exists              │
│     │                                           │
│     ├─ OnPreExecuteAsync()                     │
│     │  └─ Setup validation context             │
│     │                                           │
│     ├─ ExecuteAsync()                          │
│     │  ├─ Validate Street (not null/empty)    │
│     │  ├─ Validate City (not null/empty)      │
│     │  ├─ Validate State/ZipCode (format)     │
│     │  └─ Throw ValidationException if invalid
│     │                                           │
│     └─ OnCompleteAsync()                       │
│        └─ Cleanup validation context           │
│                                                 │
├─ 3️⃣ Log Results                               │
│  │  Logger.WriteAsync()                        │
│  │  └─ Write all collected logs for this run   │
│  │                                              │
│  └──────────────────────────────────────────────┘
│
└─ ✅ Workflow Complete!
   └─ All steps executed successfully
      OR stopped on first validation failure

Timeline View:

Time →
│
├─ T0: OnExecuteAsync() starts
│
├─ T1: GetStateAsync() completes (loads from DB)
│
├─ T2: ValidatePersonStep executes (10ms)
│
├─ T3: ValidateContactStep executes (5ms)
│
├─ T4: ValidateAddressStep executes (8ms)
│
├─ T5: Logger.WriteAsync() completes (2ms)
│
└─ T6: OnExecuteAsync() returns
   Total Duration: ~25ms (assuming no errors)
```

---

## Database Schema Visualization

```
┌─────────────────────────────────────────────────────────────────────┐
│                         SQL Server Database                          │
│                            "People" DB                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │ Table: dbo.Person                                            │ │
│  ├──────────────────────────────────────────────────────────────┤ │
│  │ Column Name      │ Type      │ Constraint   │ Index          │ │
│  ├──────────────────┼───────────┼──────────────┼────────────────┤ │
│  │ Id               │ int       │ PK, Identity │ PRIMARY KEY    │ │
│  │ FirstName        │ nvarchar  │ NOT NULL     │                │ │
│  │ LastName         │ nvarchar  │ NOT NULL     │                │ │
│  │ DateOfBirth      │ datetime2 │ NULL         │                │ │
│  │ CreatedAt        │ datetime2 │ DEFAULT NOW  │                │ │
│  │ UpdatedAt        │ datetime2 │ DEFAULT NOW  │                │ │
│  └──────────────────────────────────────────────────────────────┘ │
│         ▲                                                           │
│         │ 1:N                                                       │
│         │                                                           │
│  ┌──────┴──────────────────────────────────────────────────────┐  │
│  │ Table: dbo.ContactInfo                                     │  │
│  ├────────────────────────────────────────────────────────────┤  │
│  │ Column Name      │ Type      │ Constraint   │ Index         │  │
│  ├──────────────────┼───────────┼──────────────┼───────────────┤  │
│  │ Id               │ int       │ PK, Identity │ PRIMARY KEY   │  │
│  │ PersonId         │ int       │ FK, NOT NULL │ FOREIGN KEY   │  │
│  │ Email            │ nvarchar  │ NOT NULL     │               │  │
│  │ Phone            │ nvarchar  │ NOT NULL     │               │  │
│  │ Type             │ int       │ NOT NULL     │               │  │
│  │ CreatedAt        │ datetime2 │ DEFAULT NOW  │               │  │
│  │ UpdatedAt        │ datetime2 │ DEFAULT NOW  │               │  │
│  └────────┬──────────────────────────────────────────────────────┘ │
│           │ 1:N                                                    │
│           │                                                        │
│  ┌────────▼──────────────────────────────────────────────────┐   │
│  │ Table: dbo.Address                                        │   │
│  ├───────────────────────────────────────────────────────────┤   │
│  │ Column Name      │ Type      │ Constraint   │ Index       │   │
│  ├──────────────────┼───────────┼──────────────┼─────────────┤   │
│  │ Id               │ int       │ PK, Identity │ PRIMARY KEY │   │
│  │ ContactInfoId    │ int       │ FK, NOT NULL │ FOREIGN KEY │   │
│  │ Street           │ nvarchar  │ NOT NULL     │             │   │
│  │ City             │ nvarchar  │ NOT NULL     │             │   │
│  │ State            │ nvarchar  │ NOT NULL     │             │   │
│  │ ZipCode          │ nvarchar  │ NOT NULL     │             │   │
│  │ Country          │ nvarchar  │ NOT NULL     │             │   │
│  │ AddressType      │ int       │ NOT NULL     │             │   │
│  │ CreatedAt        │ datetime2 │ DEFAULT NOW  │             │   │
│  │ UpdatedAt        │ datetime2 │ DEFAULT NOW  │             │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                                                     │
│  Relationships (enforced by foreign keys):                         │
│  ✓ Person.Id ──── ContactInfo.PersonId (CASCADE on delete)        │
│  ✓ ContactInfo.Id ──── Address.ContactInfoId (CASCADE)            │
│                                                                     │
│  Example Query - Get all data for Person #123:                    │
│  ┌───────────────────────────────────────────────────────────┐   │
│  │ SELECT p.*, c.*, a.*                                      │   │
│  │ FROM Person p                                             │   │
│  │ LEFT JOIN ContactInfo c ON p.Id = c.PersonId             │   │
│  │ LEFT JOIN Address a ON c.Id = a.ContactInfoId            │   │
│  │ WHERE p.Id = 123                                          │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## State Flow in Workflow

```
┌──────────────────────────────────────────────────────────┐
│              PersonState (Generic State Object)           │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  IObjectAccessor<PersonState>                           │
│  {                                                       │
│      Person? Value { get; set; }                        │
│  }                                                       │
│                                                          │
│  Initial State:                                         │
│  ┌─────────────────────────────────┐                   │
│  │ PersonState                     │                   │
│  │                                 │                   │
│  │ • Person: null                  │                   │
│  │ • Errors: []                    │                   │
│  │ • IsValid: undefined            │                   │
│  └────────────┬────────────────────┘                   │
│               │                                         │
│               ▼ GetStateAsync()                        │
│  ┌─────────────────────────────────┐                   │
│  │ PersonState                     │                   │
│  │                                 │                   │
│  │ • Person: {loaded from DB}      │                   │
│  │   ├─ Id: 1                      │                   │
│  │   ├─ FirstName: "John"          │                   │
│  │   ├─ ContactInfos: []           │                   │
│  │   └─ ...                        │                   │
│  │ • Errors: []                    │                   │
│  │ • IsValid: undefined            │                   │
│  └────────────┬────────────────────┘                   │
│               │                                         │
│               ▼ Step 1: ValidatePersonStep             │
│  ┌─────────────────────────────────┐                   │
│  │ PersonState                     │                   │
│  │                                 │                   │
│  │ • Person: {with metadata}       │                   │
│  │   ├─ Id: 1                      │                   │
│  │   ├─ FirstName: "John"          │                   │
│  │   ├─ _isPersonValid: true       │◄── Set by step   │
│  │   └─ ...                        │                   │
│  │ • PersonErrors: []              │◄── Set by step   │
│  │ • IsValid: undefined            │                   │
│  └────────────┬────────────────────┘                   │
│               │                                         │
│               ▼ Step 2: ValidateContactStep            │
│  ┌─────────────────────────────────┐                   │
│  │ PersonState                     │                   │
│  │                                 │                   │
│  │ • Person: {with more metadata}  │                   │
│  │   ├─ Id: 1                      │                   │
│  │   ├─ FirstName: "John"          │                   │
│  │   ├─ _isPersonValid: true       │                   │
│  │   ├─ _isContactValid: true      │◄── Set by step   │
│  │   └─ ...                        │                   │
│  │ • PersonErrors: []              │                   │
│  │ • ContactErrors: []             │◄── Set by step   │
│  │ • IsValid: undefined            │                   │
│  └────────────┬────────────────────┘                   │
│               │                                         │
│               ▼ Step 3: ValidateAddressStep            │
│  ┌─────────────────────────────────┐                   │
│  │ PersonState                     │                   │
│  │                                 │                   │
│  │ • Person: {fully validated}     │                   │
│  │   ├─ Id: 1                      │                   │
│  │   ├─ FirstName: "John"          │                   │
│  │   ├─ _isPersonValid: true       │                   │
│  │   ├─ _isContactValid: true      │                   │
│  │   ├─ _isAddressValid: true      │◄── Set by step   │
│  │   └─ ...                        │                   │
│  │ • PersonErrors: []              │                   │
│  │ • ContactErrors: []             │                   │
│  │ • AddressErrors: []             │◄── Set by step   │
│  │ • IsValid: true                 │◄── Computed      │
│  └────────────┬────────────────────┘                   │
│               │                                         │
│               ▼ Update Database                        │
│  ┌─────────────────────────────────┐                   │
│  │ State persisted to SQL Server    │                   │
│  │                                 │                   │
│  │ INSERT/UPDATE:                  │                   │
│  │ • Person metadata               │                   │
│  │ • Validation results            │                   │
│  │ • Error logs                    │                   │
│  │ • UpdatedAt timestamp           │                   │
│  └─────────────────────────────────┘                   │
│                                                          │
│  Final State: FULLY VALIDATED & PERSISTED              │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## Testing Architecture

```
┌────────────────────────────────────────────────────────────────┐
│                    Test Pyramid Strategy                        │
└────────────────────────────────────────────────────────────────┘

                         ▲
                        ╱ ╲
                       ╱   ╲  Unit Tests
                      ╱     ╲ (PersonWorkflowTests,
                     ╱       ╲ ValidatePersonStepTests, etc)
                    ╱         ╲ Framework: NUnit + Moq
                   ╱───────────╲ Count: ~20 tests
                  ╱             ╲
                 ╱               ╲
                ╱                 ╲ Integration Tests
               ╱                   ╲ (PeopleControllerTests,
              ╱                     ╲ MultiControllerWorkflowTests)
             ╱─────────────────────╲ Framework: NUnit + Playwright
            ╱                       ╲ Count: ~15 tests
           ╱                         ╲
          ╱                           ╲
         ╱                             ╲ E2E Tests
        ╱                               ╲ (TypeScript Playwright)
       ╱                                 ╲ Framework: Playwright Test
      ╱─────────────────────────────────╲ Count: ~10 tests
     ╱                                   ╲
    ╱───────────────────────────────────╲
   ▲
   └─ Faster, Cheaper, More Frequent
      (Bottom Layer)

   Cost/Time to Run (Relative):
   Unit Tests:        ⚡ (< 1 second)
   Integration Tests: ⚡⚡⚡ (5-30 seconds)
   E2E Tests:         ⚡⚡⚡⚡⚡ (30-120 seconds)

Unit Test Example:
┌─────────────────────────────────────────┐
│ PersonWorkflowTests                     │
│                                         │
│ ✓ OnExecuteAsync_ExecutesAllSteps      │
│ ✓ OnExecuteAsync_SkipsStepsWhenFalse   │
│ ✓ OnExecuteAsync_HandlesErrors         │
│ ✓ WithCancellationToken                │
│                                         │
│ Setup: Create mocks of:                 │
│ • IWorkflowLogger<int, PersonState>    │
│ • IMessageWorkflowStep<int, PersonState>│
│                                         │
│ Execution: Direct method calls          │
│ Verification: Moq.Verify()             │
└─────────────────────────────────────────┘

Integration Test Example:
┌─────────────────────────────────────────┐
│ PeopleControllerTests                   │
│                                         │
│ ✓ Get_Returns_AllPersons               │
│ ✓ Post_CreatesNewPerson                │
│ ✓ Put_UpdatesExistingPerson            │
│ ✓ Delete_RemovesPerson                 │
│                                         │
│ Setup: Aspire app via:                  │
│ • DistributedApplicationTestingBuilder │
│ • Auto-discover WebAPI endpoint        │
│ • Create HttpClient                    │
│                                         │
│ Execution: HTTP requests                │
│ Verification: HttpStatusCode checks    │
└─────────────────────────────────────────┘

E2E Test Example:
┌─────────────────────────────────────────┐
│ people.spec.ts (TypeScript)             │
│                                         │
│ ✓ Create person via UI                 │
│ ✓ Verify person in list                │
│ ✓ Edit person details                  │
│ ✓ Delete person from UI                │
│                                         │
│ Setup: Playwright with:                 │
│ • Full browser automation               │
│ • API client for data setup             │
│ • Test data management                  │
│                                         │
│ Execution: Browser interactions         │
│ Verification: UI assertions             │
└─────────────────────────────────────────┘
```

---

## Summary

This architecture provides:

✅ **Modularity** - Clean separation of concerns across projects  
✅ **Scalability** - Stateless services, event-driven processing  
✅ **Testability** - Multiple test layers with proper isolation  
✅ **Maintainability** - Clear data flow, consistent patterns  
✅ **Observability** - Aspire dashboard, structured logging  
✅ **Cloud-Ready** - Container-based, infrastructure-as-code  

For detailed documentation, see `ARCHITECTURE.md` and `ARCHITECTURE_QUICK_REFERENCE.md`
