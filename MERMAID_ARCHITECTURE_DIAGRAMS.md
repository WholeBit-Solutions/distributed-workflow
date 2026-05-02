# KafkaWorkflow - Mermaid Architecture Diagrams

## 1. System Architecture Overview

```mermaid
graph TB
    subgraph "Presentation Layer"
        WebUI["🌐 Web Browser<br/>Scalar API Docs"]
        TestClient["🧪 Test Clients<br/>Playwright"]
    end

    subgraph "API Layer"
        PeopleCtrl["👤 PeopleController<br/>GET/POST/PUT/DELETE"]
        ContactCtrl["📧 ContactController<br/>GET/POST/PUT/DELETE"]
        AddressCtrl["🏠 AddressController<br/>GET/POST/PUT/DELETE"]
    end

    subgraph "Message Broker"
        Kafka["🔌 Apache Kafka<br/>people-topic"]
    end

    subgraph "Producer"
        Producer["📤 Kafka Producer<br/>Event Publishing"]
    end

    subgraph "Consumer & Workflow"
        ConsumerWorker["🔄 ConsumerWorker<br/>BackgroundService"]
        PersonWorkflow["🔁 PersonWorkflow<br/>Generic Workflow"]
        ValidateSteps["✅ Validation Steps<br/>Person/Contact/Address"]
    end

    subgraph "Data Access"
        DbContext["📦 PeopleContext<br/>Entity Framework"]
        Entities["🗂️ Entities<br/>Person/Contact/Address"]
    end

    subgraph "Infrastructure"
        SqlServer["🗄️ SQL Server<br/>People Database"]
        KafkaUI["📊 Kafka UI<br/>Broker Management"]
    end

    subgraph "Testing"
        UnitTests["🧪 Unit Tests<br/>NUnit + Moq"]
        IntegrationTests["🧪 Integration Tests<br/>Playwright C#"]
        E2ETests["🧪 E2E Tests<br/>Playwright TS"]
    end

    subgraph "Orchestration"
        Aspire["🎛️ Aspire Host<br/>Container Orchestration"]
    end

    WebUI -->|HTTP| PeopleCtrl
    TestClient -->|HTTP| PeopleCtrl
    
    PeopleCtrl --> DbContext
    ContactCtrl --> DbContext
    AddressCtrl --> DbContext
    
    PeopleCtrl --> Producer
    ContactCtrl --> Producer
    AddressCtrl --> Producer
    
    Producer --> Kafka
    Kafka --> ConsumerWorker
    
    ConsumerWorker --> PersonWorkflow
    PersonWorkflow --> ValidateSteps
    ValidateSteps --> DbContext
    
    DbContext --> Entities
    Entities --> SqlServer
    
    Kafka --> KafkaUI
    
    UnitTests -.-> PersonWorkflow
    IntegrationTests -.-> PeopleCtrl
    E2ETests -.-> PeopleCtrl
    
    Aspire -.-> PeopleCtrl
    Aspire -.-> ConsumerWorker
    Aspire -.-> SqlServer
    Aspire -.-> Kafka

    classDef api fill:#3498db,stroke:#2c3e50,color:#fff
    classDef workflow fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef data fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef infra fill:#f39c12,stroke:#2c3e50,color:#fff
    classDef test fill:#9b59b6,stroke:#2c3e50,color:#fff
    classDef orchestration fill:#1abc9c,stroke:#2c3e50,color:#fff
    
    class PeopleCtrl,ContactCtrl,AddressCtrl api
    class PersonWorkflow,ValidateSteps,ConsumerWorker workflow
    class DbContext,Entities,SqlServer data
    class Kafka,KafkaUI,Producer infra
    class UnitTests,IntegrationTests,E2ETests test
    class Aspire orchestration
```

---

## 2. Component Interaction & Data Flow

```mermaid
graph LR
    subgraph "Synchronous Path"
        User["👤 User"]
        Controller["📨 Controller"]
        DbCtx["📦 DbContext"]
        Db["🗄️ Database"]
        Response["✅ Response"]
    end

    subgraph "Asynchronous Path"
        Producer["📤 Producer"]
        Topic["🔌 Kafka Topic"]
        Consumer["🔄 Consumer"]
        Workflow["🔁 Workflow"]
        Steps["✅ Steps"]
    end

    User -->|1. HTTP POST| Controller
    Controller -->|2. Add Entity| DbCtx
    DbCtx -->|3. INSERT| Db
    Db -->|4. Persisted| DbCtx
    DbCtx -->|5. Return| Controller
    
    Controller -->|6. Publish Event| Producer
    Producer -->|7. Send| Topic
    Topic -->|8. Subscribe| Consumer
    Consumer -->|9. Process| Workflow
    Workflow -->|10. Execute| Steps
    Steps -->|11. Update State| Db
    
    Controller -->|12. HTTP Response| Response
    Response -->|13. Success| User

    classDef sync fill:#3498db,stroke:#2c3e50,color:#fff
    classDef async fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef storage fill:#2ecc71,stroke:#2c3e50,color:#fff
    
    class User,Controller,DbCtx,Response sync
    class Producer,Topic,Consumer,Workflow,Steps async
    class Db storage
```

---

## 3. Service Dependencies & Wiring

```mermaid
graph TB
    subgraph "AppHost.cs - Orchestration"
        AppHost["🎛️ DistributedApplication<br/>Resource Definitions"]
    end

    subgraph "SQL Server Container"
        SqlServerContainer["🗄️ SQL Server<br/>Port: 57242"]
        InitScript["📜 initSql.sql<br/>Schema & Data"]
    end

    subgraph "Kafka Container"
        KafkaContainer["🔌 Apache Kafka<br/>Port: 9092"]
        KafkaUIContainer["📊 Kafka UI<br/>Port: 8080"]
    end

    subgraph "WebAPI Service"
        WebApiProj["KafkaWorkflow.WebApi"]
        Program["Program.cs<br/>Service Registration"]
        Controllers["3x Controllers<br/>REST Endpoints"]
    end

    subgraph "Consumer Service"
        ConsumerProj["KafkaWorkflow.Consumer"]
        ConsumerProgram["Program.cs<br/>Worker Registration"]
        WorkerService["ConsumerWorker<br/>BackgroundService"]
    end

    subgraph "Shared Libraries"
        DataAccess["KafkaWorkflow.DataAccess<br/>DbContext, Entities"]
        ServiceDefaults["KafkaWorkflow.ServiceDefaults<br/>Logging, Health Checks"]
    end

    AppHost -->|Creates| SqlServerContainer
    AppHost -->|Creates| KafkaContainer
    AppHost -->|Creates| KafkaUIContainer
    AppHost -->|Starts| WebApiProj
    AppHost -->|Starts| ConsumerProj
    
    SqlServerContainer -->|Executes| InitScript
    
    WebApiProj -->|Depends| DataAccess
    WebApiProj -->|Depends| ServiceDefaults
    WebApiProj -->|References| SqlServerContainer
    WebApiProj -->|References| KafkaContainer
    
    ConsumerProj -->|Depends| DataAccess
    ConsumerProj -->|Depends| ServiceDefaults
    ConsumerProj -->|References| SqlServerContainer
    ConsumerProj -->|References| KafkaContainer
    
    Program -->|Registers| Controllers
    ConsumerProgram -->|Registers| WorkerService
    
    classDef orchestration fill:#1abc9c,stroke:#2c3e50,color:#fff
    classDef service fill:#3498db,stroke:#2c3e50,color:#fff
    classDef infrastructure fill:#f39c12,stroke:#2c3e50,color:#fff
    classDef library fill:#9b59b6,stroke:#2c3e50,color:#fff
    
    class AppHost orchestration
    class WebApiProj,ConsumerProj service
    class SqlServerContainer,KafkaContainer,KafkaUIContainer infrastructure
    class DataAccess,ServiceDefaults library
```

---

## 4. Workflow Execution Pipeline

```mermaid
graph TD
    Message["📬 Kafka Message<br/>Topic: people-topic"]
    
    Message --> ConsumerWorker["ConsumerWorker.ExecuteAsync()"]
    
    ConsumerWorker --> CreateWorkflow["Create Workflow Instance<br/>PersonWorkflow"]
    
    CreateWorkflow --> Execute["OnExecuteAsync<br/>Executes in Sequence"]
    
    Execute --> GetState["1️⃣ GetStateAsync()<br/>Load from Database"]
    
    GetState --> StateLoaded["PersonState Created<br/>Person, Contacts, Addresses"]
    
    StateLoaded --> ForEachStep["2️⃣ ForEach Step in Steps"]
    
    ForEachStep --> ShouldExecute["ShouldExecuteAsync()<br/>Check Conditions"]
    
    ShouldExecute --> |true| PreExecute["OnPreExecuteAsync()<br/>Setup & Initialization"]
    ShouldExecute --> |false| Skip["⏭️ Skip to Next Step"]
    
    PreExecute --> ExecuteStep["ExecuteAsync()<br/>Perform Business Logic"]
    
    ExecuteStep --> |Success| Complete["OnCompleteAsync()<br/>Cleanup & Finalize"]
    ExecuteStep --> |Exception| ErrorHandler["OnErrorAsync()<br/>Handle Error"]
    
    ErrorHandler --> |Continue=true| Skip
    ErrorHandler --> |Continue=false| Stop["⛔ Stop Workflow"]
    
    Complete --> NextStep{More Steps?}
    Skip --> NextStep
    
    NextStep --> |Yes| ForEachStep
    NextStep --> |No| WriteLog["Logger.WriteAsync()<br/>Persist Logs"]
    
    WriteLog --> UpdateDb["Update Database<br/>with Final State"]
    
    UpdateDb --> Complete2["✅ Workflow Complete"]
    
    Stop --> Error["❌ Workflow Failed"]
    
    classDef step1 fill:#3498db,stroke:#2c3e50,color:#fff
    classDef step2 fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef step3 fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef decision fill:#f39c12,stroke:#2c3e50,color:#fff
    classDef end fill:#27ae60,stroke:#2c3e50,color:#fff
    classDef error fill:#c0392b,stroke:#2c3e50,color:#fff
    
    class Message,ConsumerWorker,CreateWorkflow step1
    class Execute,ForEachStep step2
    class GetState,ShouldExecute,PreExecute,ExecuteStep,Complete step3
    class NextStep,ShouldExecute decision
    class Complete2 end
    class Error,Stop error
```

---

## 5. Database Entity Relationships

```mermaid
erDiagram
    PERSON ||--o{ CONTACTINFO : has
    CONTACTINFO ||--o{ ADDRESS : has

    PERSON {
        int Id PK
        string FirstName
        string LastName
        datetime DateOfBirth
        datetime CreatedAt
        datetime UpdatedAt
    }

    CONTACTINFO {
        int Id PK
        int PersonId FK
        string Email
        string Phone
        int Type
        datetime CreatedAt
        datetime UpdatedAt
    }

    ADDRESS {
        int Id PK
        int ContactInfoId FK
        string Street
        string City
        string State
        string ZipCode
        string Country
        int AddressType
        datetime CreatedAt
        datetime UpdatedAt
    }
```

---

## 6. Class Hierarchy - Workflow Framework

```mermaid
graph TD
    IMessageWorkflow["<<interface>><br/>IMessageWorkflow&lt;T, TState&gt;<br/>─────────────<br/>+ Steps<br/>+ StateAccessor<br/>+ Logger<br/>+ OnExecuteAsync"]
    
    BusinessWorkflow["<<abstract>><br/>BusinessWorkflow&lt;T, TState&gt;<br/>─────────────<br/>+ OnExecuteAsync<br/>+ OnGetStateAsync<br/>+ Steps<br/>+ StateAccessor<br/>+ Logger"]
    
    PersonWorkflow["PersonWorkflow<br/>: BusinessWorkflow&lt;int, PersonState&gt;<br/>─────────────<br/>+ OnGetStateAsync<br/>+ PersonState<br/>+ Validation Steps"]
    
    IMessageWorkflowStep["<<interface>><br/>IMessageWorkflowStep&lt;T, TState&gt;<br/>─────────────<br/>+ ShouldExecuteAsync<br/>+ OnPreExecuteAsync<br/>+ ExecuteAsync<br/>+ OnCompleteAsync<br/>+ OnErrorAsync"]
    
    MessageWorkflowStep["<<abstract>><br/>MessageWorkflowStep&lt;T, TState&gt;<br/>─────────────<br/>+ IObjectAccessor<br/>+ IWorkflowLogger<br/>+ Protected Methods"]
    
    ValidatePersonStep["ValidatePersonStep<br/>: MessageWorkflowStep&lt;int, PersonState&gt;<br/>─────────────<br/>+ Validates Person Data"]
    
    ValidateContactStep["ValidateContactStep<br/>: MessageWorkflowStep&lt;int, PersonState&gt;<br/>─────────────<br/>+ Validates Contact Data"]
    
    ValidateAddressStep["ValidateAddressStep<br/>: MessageWorkflowStep&lt;int, PersonState&gt;<br/>─────────────<br/>+ Validates Address Data"]
    
    IMessageWorkflow --> BusinessWorkflow
    BusinessWorkflow --> PersonWorkflow
    
    IMessageWorkflowStep --> MessageWorkflowStep
    MessageWorkflowStep --> ValidatePersonStep
    MessageWorkflowStep --> ValidateContactStep
    MessageWorkflowStep --> ValidateAddressStep
    
    PersonWorkflow -.->|contains| ValidatePersonStep
    PersonWorkflow -.->|contains| ValidateContactStep
    PersonWorkflow -.->|contains| ValidateAddressStep
    
    classDef interface fill:#6f42c1,stroke:#2c3e50,color:#fff
    classDef abstract fill:#9b59b6,stroke:#2c3e50,color:#fff
    classDef concrete fill:#e74c3c,stroke:#2c3e50,color:#fff
    
    class IMessageWorkflow,IMessageWorkflowStep interface
    class BusinessWorkflow,MessageWorkflowStep abstract
    class PersonWorkflow,ValidatePersonStep,ValidateContactStep,ValidateAddressStep concrete
```

---

## 7. REST API Endpoints

```mermaid
graph TB
    subgraph "PeopleController"
        Get["GET /people<br/>Returns all people<br/>✅ 200 OK"]
        GetById["GET /people/{id}<br/>Returns person by ID<br/>✅ 200 OK | ❌ 404 Not Found"]
        Post["POST /people<br/>Creates new person<br/>✅ 201 Created | ❌ 400 Bad Request"]
        Put["PUT /people/{id}<br/>Updates person<br/>✅ 200 OK | ❌ 404 Not Found"]
        Delete["DELETE /people/{id}<br/>Deletes person<br/>✅ 200 OK | ❌ 404 Not Found"]
    end

    subgraph "ContactController"
        GetContact["GET /contact<br/>Returns all contacts"]
        PostContact["POST /contact<br/>Creates new contact"]
        PutContact["PUT /contact<br/>Updates contact"]
        DeleteContact["DELETE /contact/{id}<br/>Deletes contact"]
    end

    subgraph "AddressController"
        GetAddress["GET /address<br/>Returns all addresses"]
        PostAddress["POST /address<br/>Creates new address"]
        PutAddress["PUT /address<br/>Updates address"]
        DeleteAddress["DELETE /address/{id}<br/>Deletes address"]
    end

    subgraph "Business Logic"
        DbInsert["INSERT to Database"]
        PublishEvent["Publish to Kafka<br/>Topic: people-topic"]
        Response["HTTP Response<br/>with Location Header"]
    end

    Post -->|1. Save| DbInsert
    DbInsert -->|2. Success| PublishEvent
    PublishEvent -->|3. Return| Response
    Response -->|201 Created| Client["📱 Client"]

    PostContact -->|1. Save| DbInsert
    PutContact -->|1. Update| DbInsert
    DeleteContact -->|1. Remove| DbInsert
    PostAddress -->|1. Save| DbInsert
    PutAddress -->|1. Update| DbInsert
    DeleteAddress -->|1. Remove| DbInsert

    classDef endpoint fill:#3498db,stroke:#2c3e50,color:#fff
    classDef success fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef error fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef logic fill:#f39c12,stroke:#2c3e50,color:#fff
    
    class Get,GetById,Post,Put,Delete,GetContact,PostContact,PutContact,DeleteContact,GetAddress,PostAddress,PutAddress,DeleteAddress endpoint
    class DbInsert,PublishEvent,Response logic
```

---

## 8. Testing Architecture Pyramid

```mermaid
graph TD
    Pyramid["Testing Pyramid"]
    
    subgraph "Unit Tests (Bottom - Fast)"
        UT1["PersonWorkflowTests<br/>✓ 4 test cases"]
        UT2["ValidatePersonStepTests<br/>✓ 3 test cases"]
        UT3["ValidateContactStepTests<br/>✓ 3 test cases"]
        UT4["ValidateAddressStepTests<br/>✓ 3 test cases"]
    end
    
    subgraph "Integration Tests (Middle - Medium)"
        IT1["PeopleControllerTests<br/>✓ 5 test cases"]
        IT2["ContactControllerTests<br/>✓ 5 test cases"]
        IT3["AddressControllerTests<br/>✓ 5 test cases"]
        IT4["MultiControllerWorkflowTests<br/>✓ 3 test cases"]
        IT5["PeopleControllerErrorHandlingTests<br/>✓ 4 test cases"]
    end
    
    subgraph "E2E Tests (Top - Slow)"
        E2E1["people.spec.ts<br/>✓ Playwright"]
        E2E2["contact.spec.ts<br/>✓ Playwright"]
        E2E3["address.spec.ts<br/>✓ Playwright"]
    end
    
    UT1 --> Framework1["Framework:<br/>NUnit + Moq"]
    UT2 --> Framework1
    UT3 --> Framework1
    UT4 --> Framework1
    
    IT1 --> Framework2["Framework:<br/>NUnit + Playwright<br/>+ Aspire"]
    IT2 --> Framework2
    IT3 --> Framework2
    IT4 --> Framework2
    IT5 --> Framework2
    
    E2E1 --> Framework3["Framework:<br/>Playwright<br/>TypeScript"]
    E2E2 --> Framework3
    E2E3 --> Framework3
    
    Framework1 -.->|13 Tests| Cost1["⚡ Cost: Low<br/>Time: &lt;1s"]
    Framework2 -.->|22 Tests| Cost2["⚡⚡⚡ Cost: Medium<br/>Time: 5-30s"]
    Framework3 -.->|~10 Tests| Cost3["⚡⚡⚡⚡⚡ Cost: High<br/>Time: 30-120s"]
    
    classDef unittests fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef integration fill:#f39c12,stroke:#2c3e50,color:#fff
    classDef e2e fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef framework fill:#3498db,stroke:#2c3e50,color:#fff
    classDef cost fill:#9b59b6,stroke:#2c3e50,color:#fff
    
    class UT1,UT2,UT3,UT4 unittests
    class IT1,IT2,IT3,IT4,IT5 integration
    class E2E1,E2E2,E2E3 e2e
    class Framework1,Framework2,Framework3 framework
    class Cost1,Cost2,Cost3 cost
```

---

## 9. Aspire Dashboard Resources

```mermaid
graph TB
    Dashboard["🎛️ Aspire Dashboard<br/>http://localhost:15251"]
    
    Dashboard --> Resources["📊 Resources Tab"]
    Dashboard --> Logs["📝 Logs Tab"]
    Dashboard --> Traces["🔍 Traces Tab"]
    
    Resources --> WebAPI["webapi<br/>ProjectResource<br/>State: Running<br/>Port: 5000/5001<br/>Depends: db, kafka"]
    Resources --> Consumer["kafka-consumer<br/>ProjectResource<br/>State: Running<br/>Depends: db, kafka"]
    Resources --> SqlServer["database<br/>ContainerResource<br/>State: Healthy<br/>Port: 57242<br/>Volume: sqlserver-data"]
    Resources --> Kafka["kafka<br/>ContainerResource<br/>State: Healthy<br/>Port: 9092<br/>Volume: kafka-data"]
    Resources --> KafkaUI["kafka-ui<br/>ContainerResource<br/>UI: http://localhost:8080"]
    
    Logs --> WebAPILogs["WebAPI Logs"]
    Logs --> ConsumerLogs["Consumer Logs"]
    Logs --> DbLogs["Database Logs"]
    Logs --> KafkaLogs["Kafka Logs"]
    
    Traces --> WebAPITraces["HTTP Request Traces"]
    Traces --> ConsumerTraces["Workflow Execution Traces"]
    Traces --> DbTraces["Database Query Traces"]
    
    classDef dashboard fill:#1abc9c,stroke:#2c3e50,color:#fff
    classDef running fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef healthy fill:#27ae60,stroke:#2c3e50,color:#fff
    classDef logs fill:#3498db,stroke:#2c3e50,color:#fff
    classDef traces fill:#9b59b6,stroke:#2c3e50,color:#fff
    
    class Dashboard dashboard
    class WebAPI,Consumer running
    class SqlServer,Kafka,KafkaUI healthy
    class WebAPILogs,ConsumerLogs,DbLogs,KafkaLogs logs
    class WebAPITraces,ConsumerTraces,DbTraces traces
```

---

## 10. Complete System Flow - Create Person Sequence

```mermaid
sequenceDiagram
    participant User as 👤 User/Client
    participant API as 🌐 WebAPI
    participant DB as 🗄️ Database
    participant KafkaP as 📤 Kafka Producer
    participant Topic as 🔌 Kafka Topic
    participant Consumer as 🔄 Consumer
    participant Workflow as 🔁 Workflow
    participant Steps as ✅ Validation Steps

    User->>API: POST /people<br/>{firstName, lastName}
    
    API->>API: 1. Parse Request
    API->>API: 2. Create Person Entity
    
    API->>DB: 3. Add & SaveChanges
    DB-->>API: 4. Person Saved<br/>(ID: 123)
    
    API->>KafkaP: 5. ProduceAsync(event)
    KafkaP->>Topic: 6. Publish Event<br/>{Key: "123",<br/>Value: "Create"}
    
    API-->>User: 7. 201 Created<br/>{id: 123, ...}
    
    Topic->>Consumer: 8. Message Available
    Consumer->>Workflow: 9. Create & Execute<br/>Workflow(123)
    
    Workflow->>DB: 10. Load Person(123)
    DB-->>Workflow: 11. Person + Relations
    
    Workflow->>Steps: 12. Execute<br/>ValidatePersonStep
    Steps->>Steps: 13. Validate Data
    Steps-->>Workflow: 14. ✅ Valid
    
    Workflow->>Steps: 15. Execute<br/>ValidateContactStep
    Steps->>Steps: 16. Validate Data
    Steps-->>Workflow: 17. ✅ Valid
    
    Workflow->>Steps: 18. Execute<br/>ValidateAddressStep
    Steps->>Steps: 19. Validate Data
    Steps-->>Workflow: 20. ✅ Valid
    
    Workflow->>DB: 21. Update State
    DB-->>Workflow: 22. Updated
    
    Workflow-->>Consumer: 23. Complete ✅

    classDef sync fill:#3498db,stroke:#2c3e50,color:#fff
    classDef async fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef storage fill:#2ecc71,stroke:#2c3e50,color:#fff
    
    class User,API,KafkaP sync
    class Consumer,Workflow,Steps async
    class DB,Topic storage
```

---

## 11. Kafka Message Flow

```mermaid
graph LR
    subgraph "Producer Side"
        Controller["PeopleController"]
        Event["Event Created<br/>Key: PersonId<br/>Value: OperationType"]
        ProducerInstance["IProducer&lt;string, string&gt;"]
    end
    
    subgraph "Kafka Broker"
        Topic["people-topic<br/>Partitions: 1<br/>Replication: 1"]
        PartitionZero["Partition 0<br/>Offset: 0..N"]
    end
    
    subgraph "Consumer Side"
        ConsumerInstance["IConsumer&lt;string, string&gt;"]
        ConsumerGroup["Consumer Group:<br/>people-consumer-group"]
        Worker["ConsumerWorker<br/>BackgroundService"]
    end
    
    subgraph "Processing"
        Deserialization["Deserialize Message"]
        PersonWorkflow["PersonWorkflow<br/>Processing"]
        Validation["Validation Steps"]
    end
    
    Controller -->|Create Message| Event
    Event -->|Send| ProducerInstance
    ProducerInstance -->|Publish| Topic
    
    Topic -->|Store| PartitionZero
    
    ConsumerInstance -.->|Subscribe| Topic
    ConsumerInstance -->|Assign| ConsumerGroup
    ConsumerGroup -->|Read| PartitionZero
    
    PartitionZero -->|Consume| Worker
    Worker -->|Deserialize| Deserialization
    Deserialization -->|Process| PersonWorkflow
    PersonWorkflow -->|Execute| Validation
    
    classDef producer fill:#3498db,stroke:#2c3e50,color:#fff
    classDef broker fill:#f39c12,stroke:#2c3e50,color:#fff
    classDef consumer fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef processing fill:#2ecc71,stroke:#2c3e50,color:#fff
    
    class Controller,ProducerInstance,Event producer
    class Topic,PartitionZero broker
    class ConsumerInstance,ConsumerGroup,Worker consumer
    class Deserialization,PersonWorkflow,Validation processing
```

---

## 12. Dependency Injection Container

```mermaid
graph TB
    subgraph "WebAPI - Program.cs"
        WebApiServices["Service Registration"]
        
        WebApiServices -->|AddScoped| PeopleContext["PeopleContext<br/>DbContext"]
        WebApiServices -->|AddScoped| Producer["IProducer&lt;string, string&gt;<br/>Kafka Producer"]
        WebApiServices -->|AddControllers| Controllers["API Controllers"]
        WebApiServices -->|AddHealthChecks| HealthChecks["Health Check Endpoints"]
        WebApiServices -->|Configure| Logging["Logging & Telemetry"]
    end
    
    subgraph "Consumer - Program.cs"
        ConsumerServices["Service Registration"]
        
        ConsumerServices -->|AddScoped| PeopleContext2["PeopleContext<br/>DbContext"]
        ConsumerServices -->|AddScoped| Consumer["IConsumer&lt;string, string&gt;<br/>Kafka Consumer"]
        ConsumerServices -->|AddScoped| PersonWorkflow["PersonWorkflow<br/>Workflow"]
        ConsumerServices -->|AddScoped| Steps["Validation Steps<br/>DI for Steps"]
        ConsumerServices -->|AddHostedService| ConsumerWorker["ConsumerWorker<br/>BackgroundService"]
        ConsumerServices -->|Configure| Logging2["Logging & Telemetry"]
    end
    
    subgraph "ServiceDefaults - Extensions.cs"
        Extensions["Extension Methods"]
        Extensions -->|ConfigureLogging| Serilog["Serilog Setup"]
        Extensions -->|AddHealthChecks| DbHealth["Database Health Check"]
        Extensions -->|AddHealthChecks| KafkaHealth["Kafka Health Check"]
        Extensions -->|AddTelemetry| OpenTelemetry["OpenTelemetry"]
    end
    
    WebApiServices -->|Uses| Extensions
    ConsumerServices -->|Uses| Extensions
    
    classDef registration fill:#3498db,stroke:#2c3e50,color:#fff
    classDef service fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef extension fill:#9b59b6,stroke:#2c3e50,color:#fff
    
    class WebApiServices,ConsumerServices registration
    class PeopleContext,Producer,Consumer,PersonWorkflow,Steps,ConsumerWorker service
    class Extensions,Serilog,DbHealth,KafkaHealth,OpenTelemetry extension
```

---

## 13. State Management in Workflow

```mermaid
stateDiagram-v2
    [*] --> Initial
    
    Initial: PersonState Created
    Initial: Value: null
    Initial: Errors: []
    
    Initial --> LoadState
    
    LoadState: GetStateAsync()
    LoadState: Load Person from DB
    LoadState: Populate ContactInfos
    LoadState: Populate Addresses
    
    LoadState --> PersonStateLoaded
    
    PersonStateLoaded: Person Fully Loaded
    PersonStateLoaded: Value: {Person object}
    PersonStateLoaded: Errors: []
    PersonStateLoaded: IsValid: undefined
    
    PersonStateLoaded --> ValidatePersonStep
    
    ValidatePersonStep: ValidatePersonStep Executing
    ValidatePersonStep: Check FirstName/LastName
    ValidatePersonStep: Check DateOfBirth
    ValidatePersonStep: Set _isPersonValid
    
    ValidatePersonStep --> PersonValidated
    
    PersonValidated: Person Validation Done
    PersonValidated: _isPersonValid: true/false
    PersonValidated: PersonErrors: []
    
    PersonValidated --> ValidateContactStep
    
    ValidateContactStep: ValidateContactStep Executing
    ValidateContactStep: Check Email Format
    ValidateContactStep: Check Phone Format
    ValidateContactStep: Set _isContactValid
    
    ValidateContactStep --> ContactValidated
    
    ContactValidated: Contact Validation Done
    ContactValidated: _isContactValid: true/false
    ContactValidated: ContactErrors: []
    
    ContactValidated --> ValidateAddressStep
    
    ValidateAddressStep: ValidateAddressStep Executing
    ValidateAddressStep: Check Street/City
    ValidateAddressStep: Check State/ZipCode
    ValidateAddressStep: Set _isAddressValid
    
    ValidateAddressStep --> AddressValidated
    
    AddressValidated: Address Validation Done
    AddressValidated: _isAddressValid: true/false
    AddressValidated: AddressErrors: []
    
    AddressValidated --> AllValidated
    
    AllValidated: All Steps Complete
    AllValidated: IsValid: true
    AllValidated: Ready to Persist
    
    AllValidated --> PersistState
    
    PersistState: Update Database
    PersistState: Write Validation Results
    PersistState: Update UpdatedAt
    
    PersistState --> [*]
    
    Note: Error handling available in OnErrorAsync()
    Note: Can stop execution if shouldContinue=false
```

---

## 14. Project Dependencies Map

```mermaid
graph TB
    subgraph "Test Projects"
        UnitTest["KafkaWorkflow.Test<br/>Unit Tests"]
        IntegrationTest["KafkaWorkflow.PlaywrightTests<br/>Integration Tests<br/>C# + Playwright"]
        E2ETest["KafkaWorkflow.PlaywrightTests.TypeScript<br/>E2E Tests<br/>TypeScript"]
    end
    
    subgraph "Application Projects"
        WebApi["KafkaWorkflow.WebApi<br/>ASP.NET Core API"]
        Consumer["KafkaWorkflow.Consumer<br/>Background Worker"]
    end
    
    subgraph "Shared Projects"
        DataAccess["KafkaWorkflow.DataAccess<br/>EF Core DbContext"]
        ServiceDefaults["KafkaWorkflow.ServiceDefaults<br/>Shared Configuration"]
    end
    
    subgraph "Orchestration"
        AppHost["KafkaWorkflow.AppHost<br/>Aspire Host"]
    end
    
    UnitTest -->|Tests| Consumer
    IntegrationTest -->|Tests| WebApi
    E2ETest -->|Tests| WebApi
    
    WebApi -->|Uses| DataAccess
    WebApi -->|Uses| ServiceDefaults
    
    Consumer -->|Uses| DataAccess
    Consumer -->|Uses| ServiceDefaults
    
    AppHost -->|Orchestrates| WebApi
    AppHost -->|Orchestrates| Consumer
    AppHost -->|Manages| SqlServer["SQL Server<br/>Database"]
    AppHost -->|Manages| Kafka["Apache Kafka<br/>Broker"]
    
    WebApi -.->|References| AppHost
    Consumer -.->|References| AppHost
    
    classDef test fill:#9b59b6,stroke:#2c3e50,color:#fff
    classDef application fill:#3498db,stroke:#2c3e50,color:#fff
    classDef shared fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef orchestration fill:#1abc9c,stroke:#2c3e50,color:#fff
    classDef external fill:#f39c12,stroke:#2c3e50,color:#fff
    
    class UnitTest,IntegrationTest,E2ETest test
    class WebApi,Consumer application
    class DataAccess,ServiceDefaults shared
    class AppHost orchestration
    class SqlServer,Kafka external
```

---

## 15. Error Handling Flow

```mermaid
graph TD
    Step["🔄 Step Executing<br/>ExecuteAsync()"]
    
    Step --> Execute{"Execution<br/>Successful?"}
    
    Execute -->|Yes| Success["✅ Success<br/>OnCompleteAsync()"]
    Execute -->|No| Exception["❌ Exception Thrown"]
    
    Exception --> ErrorHandler["OnErrorAsync(ex)<br/>Handle the Error"]
    
    ErrorHandler --> Log["📝 Log Error<br/>Logger.CollectAsync()"]
    
    Log --> Decision{"Continue<br/>Workflow?"}
    
    Decision -->|true| Continue["⏭️ Continue<br/>Next Step"]
    Decision -->|false| Stop["🛑 Stop Workflow"]
    
    Success --> LogSuccess["📝 Log Success<br/>Logger.CollectAsync()"]
    LogSuccess --> MoreSteps{"More Steps?"}
    
    Continue --> MoreSteps
    
    MoreSteps -->|Yes| Step
    MoreSteps -->|No| Finalize["🔚 Finalize Workflow"]
    
    Stop --> Finalize
    
    Finalize --> WriteLog["📝 Write All Logs<br/>Logger.WriteAsync()"]
    WriteLog --> UpdateDb["💾 Update Database"]
    UpdateDb --> End["✅ Complete"]
    
    classDef execution fill:#3498db,stroke:#2c3e50,color:#fff
    classDef error fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef success fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef logging fill:#f39c12,stroke:#2c3e50,color:#fff
    classDef decision fill:#9b59b6,stroke:#2c3e50,color:#fff
    
    class Step,Execute execution
    class Exception,ErrorHandler,Stop error
    class Success,Continue,End success
    class Log,LogSuccess,WriteLog logging
    class Decision,MoreSteps decision
```

---

## Summary

These diagrams provide comprehensive visualization of the KafkaWorkflow architecture:

1. **System Overview** - High-level component layout
2. **Data Flow** - Sync and async request paths
3. **Service Dependencies** - How services reference each other
4. **Workflow Pipeline** - Step-by-step workflow execution
5. **Database** - Entity relationships and schema
6. **Class Hierarchy** - OOP structure of workflow framework
7. **API Endpoints** - REST endpoints and operations
8. **Testing** - Test pyramid and organization
9. **Aspire Dashboard** - Monitoring and resource management
10. **Sequence Diagram** - Complete create person flow
11. **Kafka Flow** - Message production and consumption
12. **DI Container** - Service registration and configuration
13. **State Management** - Workflow state transitions
14. **Project Dependencies** - Project references and relationships
15. **Error Handling** - Exception handling and recovery flow

All diagrams are created using Mermaid and can be rendered in markdown viewers, GitHub, GitLab, and many other platforms.
