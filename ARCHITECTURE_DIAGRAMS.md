# KafkaWorkflow - Architecture Diagrams

## 1. System Architecture Overview

```mermaid
flowchart-elk TB
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
        PeopleTopic["🔌 Kafka Topic<br/>people-topic"]
        ContactTopic["🔌 Kafka Topic<br/>contact-topic"]
        AddressTopic["🔌 Kafka Topic<br/>address-topic"]
    end

    subgraph "Producer"
        KafkaProducer["📤 Kafka Producer<br/>IProducer&lt;int, T&gt;<br/>Event Publishing"]
    end

    subgraph "Consumer & Workflow"
        PeopleWorker["🔄 PeopleWorker<br/>BackgroundService"]
        ContactWorker["🔄 ContactWorker<br/>BackgroundService"]
        AddressWorker["🔄 AddressWorker<br/>BackgroundService"]
        PersonWorkflow["🔁 PersonWorkflow<br/>Unified Orchestrator"]
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

    Producer --> PeopleTopic
    Producer --> ContactTopic
    Producer --> AddressTopic

    PeopleTopic --> PeopleWorker
    ContactTopic --> ContactWorker
    AddressTopic --> AddressWorker

    PeopleWorker --> PersonWorkflow
    ContactWorker --> PersonWorkflow
    AddressWorker --> PersonWorkflow

    PersonWorkflow --> ValidateSteps
    ValidateSteps --> DbContext

    DbContext --> Entities
    Entities --> SqlServer

    PeopleTopic --> KafkaUI
    ContactTopic --> KafkaUI
    AddressTopic --> KafkaUI
    
    UnitTests -.-> PersonWorkflow
    IntegrationTests -.-> PeopleCtrl
    
    Aspire -.-> PeopleCtrl
    Aspire -.-> AddressWorker
    Aspire -.-> PeopleWorker
    Aspire -.-> ContactWorker
    Aspire -.-> SqlServer
    Aspire -.-> Kafka

    classDef api fill:#3498db,stroke:#2c3e50,color:#fff
    classDef workflow fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef data fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef infra fill:#f39c12,stroke:#2c3e50,color:#fff
    classDef test fill:#9b59b6,stroke:#2c3e50,color:#fff
    classDef orchestration fill:#1abc9c,stroke:#2c3e50,color:#fff

    class PeopleCtrl,ContactCtrl,AddressCtrl api
    class PersonWorkflow,ValidateSteps,PeopleWorker,ContactWorker,AddressWorker workflow
    class DbContext,Entities,SqlServer data
    class PeopleTopic,ContactTopic,AddressTopic,KafkaUI,Producer infra
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
        SqlServerContainer["🗄️ SQL Server<br/>Port: 55245"]
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
        PeopleWorkerSvc["PeopleWorker<br/>BackgroundService"]
        ContactWorkerSvc["ContactWorker<br/>BackgroundService"]
        AddressWorkerSvc["AddressWorker<br/>BackgroundService"]
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
    ConsumerProgram -->|Registers| PeopleWorkerSvc
    ConsumerProgram -->|Registers| ContactWorkerSvc
    ConsumerProgram -->|Registers| AddressWorkerSvc
    
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
    classDef END fill:#27ae60,stroke:#2c3e50,color:#fff
    classDef error fill:#c0392b,stroke:#2c3e50,color:#fff
    
    class Message,ConsumerWorker,CreateWorkflow step1
    class Execute,ForEachStep step2
    class GetState,ShouldExecute,PreExecute,ExecuteStep,Complete step3
    class NextStep,ShouldExecute decision
    class Complete2 END
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
    }

    ADDRESS {
        int Id PK
        int ContactInfoId FK
        string Street
        string City
        string State
        string ZipCode
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
        GetById["GET /people/{personId}<br/>Returns person by ID<br/>✅ 200 OK | ❌ 404 Not Found"]
        Post["POST /people<br/>Creates new person<br/>Body: {firstName, lastName, age}<br/>✅ 201 Created | ❌ 400 Bad Request"]
        Put["PUT /people<br/>Updates person<br/>Body: {id, firstName, lastName, age}<br/>✅ 200 OK | ❌ 404 Not Found"]
        Delete["DELETE /people/{id}<br/>Deletes person<br/>✅ 200 OK | ❌ 404 Not Found"]
    end

    subgraph "ContactController"
        GetContact["GET /contact<br/>Returns all contacts<br/>✅ 200 OK"]
        GetContactById["GET /contact/{contactInfoId}<br/>Returns contact by ID<br/>✅ 200 OK | ❌ 404 Not Found"]
        PostContact["POST /contact/{personId}<br/>Creates new contact<br/>Route: personId<br/>Body: {email, phone}<br/>✅ 201 Created | ❌ 400/404"]
        PutContact["PUT /contact<br/>Updates contact<br/>Body: {id, email, phone, personId}<br/>✅ 200 OK | ❌ 404 Not Found"]
        DeleteContact["DELETE /contact/{id}<br/>Deletes contact<br/>✅ 200 OK | ❌ 404 Not Found"]
    end

    subgraph "AddressController"
        GetAddress["GET /address<br/>Returns all addresses<br/>✅ 200 OK"]
        GetAddressById["GET /address/{addressId}<br/>Returns address by ID<br/>✅ 200 OK | ❌ 404 Not Found"]
        PostAddress["POST /address/{contactInfoId}<br/>Creates new address<br/>Route: contactInfoId<br/>Body: {street, city, state, zipCode}<br/>✅ 201 Created | ❌ 400/404"]
        PutAddress["PUT /address<br/>Updates address<br/>Body: {id, street, city, state, zipCode, contactInfoId}<br/>✅ 200 OK | ❌ 404 Not Found"]
        DeleteAddress["DELETE /address/{id}<br/>Deletes address<br/>✅ 200 OK | ❌ 404 Not Found"]
    end

    subgraph "Business Logic Flow"
        DbInsert["1️⃣ INSERT to Database<br/>SaveChangesAsync()"]
        PublishEvent["2️⃣ Publish to Kafka<br/>Topic: people-topic<br/>Topic: contact-topic<br/>Topic: address-topic"]
        Response["3️⃣ HTTP Response<br/>Location: /resource/{id}"]
    end

    Post -->|Execute| DbInsert
    PostContact -->|Execute| DbInsert
    PostAddress -->|Execute| DbInsert
    Put -->|Execute| DbInsert
    PutContact -->|Execute| DbInsert
    PutAddress -->|Execute| DbInsert
    Delete -->|Execute| DbInsert
    DeleteContact -->|Execute| DbInsert
    DeleteAddress -->|Execute| DbInsert

    DbInsert -->|Success| PublishEvent
    PublishEvent -->|Complete| Response
    Response -->|201 Created| Client["📱 Client"]

    classDef endpoint fill:#3498db,stroke:#2c3e50,color:#fff
    classDef getEndpoint fill:#27ae60,stroke:#2c3e50,color:#fff
    classDef success fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef error fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef logic fill:#f39c12,stroke:#2c3e50,color:#fff

    class Post,PostContact,PostAddress,Put,PutContact,PutAddress,Delete,DeleteContact,DeleteAddress endpoint
    class Get,GetById,GetContact,GetContactById,GetAddress,GetAddressById getEndpoint
    class DbInsert,PublishEvent logic
    class Response success
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
    Resources --> SqlServer["database<br/>ContainerResource<br/>State: Healthy<br/>Port: 55245<br/>Volume: sqlserver-data"]
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
    participant API as 🌐 WebAPI<br/>PeopleController
    participant DB as 🗄️ SQL Server<br/>Database
    participant KafkaP as 📤 Kafka Producer<br/>IProducer
    participant Topic as 🔌 Kafka Broker<br/>people-topic
    participant Worker as 🔄 PeopleWorker<br/>BackgroundService
    participant Workflow as 🔁 PersonWorkflow<br/>Orchestrator
    participant Steps as ✅ Validation Steps<br/>3-step pipeline

    User->>API: 1. POST /people<br/>{firstName, lastName,<br/>age}

    API->>API: 2. Parse & Validate<br/>Request Body
    API->>API: 3. Create Person<br/>Entity Instance

    API->>DB: 4. context.Persons.Add()<br/>SaveChangesAsync()
    DB-->>API: 5. ✅ Inserted<br/>(ID: 123)

    API->>KafkaP: 6. Create Message<br/>Key: 123<br/>Value: Person object
    KafkaP->>Topic: 7. ProduceAsync()<br/>Publish Event
    Topic-->>KafkaP: 8. ✅ Published<br/>Offset: N

    API-->>User: 9. 201 Created<br/>Location: /people/123<br/>Body: {id: 123, ...}

    Topic->>Worker: 10. Message Available<br/>Consume()
    Worker->>Workflow: 11. new PersonWorkflow()<br/>OnExecuteAsync(123)

    Workflow->>DB: 12. GetStateAsync()<br/>Load Person(123)<br/>Include ContactInfos<br/>ThenInclude Addresses
    DB-->>Workflow: 13. PersonState<br/>{Person, Contacts,<br/>Addresses}

    Workflow->>Steps: 14. Execute Step 1<br/>ValidatePersonStep
    Steps->>Steps: 15. Validate<br/>FirstName ✓<br/>LastName ✓<br/>Age ✓
    Steps-->>Workflow: 16. ✅ Step 1 Complete<br/>OnCompleteAsync()

    Workflow->>Steps: 17. Execute Step 2<br/>ValidateContactStep
    Steps->>Steps: 18. Validate<br/>Email format ✓<br/>Phone format ✓<br/>PersonId exists ✓
    Steps-->>Workflow: 19. ✅ Step 2 Complete<br/>OnCompleteAsync()

    Workflow->>Steps: 20. Execute Step 3<br/>ValidateAddressStep
    Steps->>Steps: 21. Validate<br/>Street ✓<br/>City ✓<br/>State ✓<br/>ZipCode ✓
    Steps-->>Workflow: 22. ✅ Step 3 Complete<br/>OnCompleteAsync()

    Workflow->>DB: 23. Update PersonState<br/>SaveChangesAsync()
    DB-->>Workflow: 24. ✅ State Updated

    Workflow-->>Worker: 25. ✅ Workflow Complete
```

---

## 11. Kafka Message Flow

```mermaid
graph LR
    subgraph "Producer Side"
        PeopleCtrl["PeopleController<br/>Create/Update/Delete"]
        ContactCtrl["ContactController<br/>Create/Update/Delete"]
        AddressCtrl["AddressController<br/>Create/Update/Delete"]
        Event["Events Created<br/>Key: PersonId<br/>Value: Entity Object"]
        ProducerInstance["IProducer&lt;int, T&gt;<br/>Kafka Producer"]
    end

    subgraph "Kafka Broker"
        PeopleTopic["people-topic<br/>Partitions: 1<br/>Replication: 1"]
        ContactTopic["contact-topic<br/>Partitions: 1<br/>Replication: 1"]
        AddressTopic["address-topic<br/>Partitions: 1<br/>Replication: 1"]

        PeoplePartition["Partition 0<br/>Offset: 0..N"]
        ContactPartition["Partition 0<br/>Offset: 0..N"]
        AddressPartition["Partition 0<br/>Offset: 0..N"]
    end

    subgraph "Consumer Side"
        PeopleConsumer["IConsumer&lt;int, Person&gt;<br/>people-consumer-group"]
        ContactConsumer["IConsumer&lt;int, ContactInfo&gt;<br/>people-consumer-group"]
        AddressConsumer["IConsumer&lt;int, Address&gt;<br/>people-consumer-group"]
    end

    subgraph "Worker Processing"
        PeopleWorker["PeopleWorker<br/>BackgroundService"]
        ContactWorker["ContactWorker<br/>BackgroundService"]
        AddressWorker["AddressWorker<br/>BackgroundService"]
    end

    subgraph "Workflow Processing"
        PersonWorkflow["PersonWorkflow<br/>Orchestrator"]
        ValidationSteps["Validation Steps<br/>Person/Contact/Address"]
    end

    PeopleCtrl -->|Person Event| Event
    ContactCtrl -->|ContactInfo Event| Event
    AddressCtrl -->|Address Event| Event

    Event -->|Send| ProducerInstance

    ProducerInstance -->|Publish| PeopleTopic
    ProducerInstance -->|Publish| ContactTopic
    ProducerInstance -->|Publish| AddressTopic

    PeopleTopic -->|Store| PeoplePartition
    ContactTopic -->|Store| ContactPartition
    AddressTopic -->|Store| AddressPartition

    PeopleConsumer -.->|Subscribe| PeopleTopic
    ContactConsumer -.->|Subscribe| ContactTopic
    AddressConsumer -.->|Subscribe| AddressTopic

    PeoplePartition -->|Consume| PeopleWorker
    ContactPartition -->|Consume| ContactWorker
    AddressPartition -->|Consume| AddressWorker

    PeopleWorker -->|Execute| PersonWorkflow
    ContactWorker -->|Execute| PersonWorkflow
    AddressWorker -->|Execute| PersonWorkflow

    PersonWorkflow -->|Run Steps| ValidationSteps
    ValidationSteps -->|Update State| DB["🗄️ Database"]

    classDef producer fill:#3498db,stroke:#2c3e50,color:#fff
    classDef broker fill:#f39c12,stroke:#2c3e50,color:#fff
    classDef consumer fill:#e74c3c,stroke:#2c3e50,color:#fff
    classDef processing fill:#2ecc71,stroke:#2c3e50,color:#fff

    class PeopleCtrl,ContactCtrl,AddressCtrl,ProducerInstance,Event producer
    class PeopleTopic,ContactTopic,AddressTopic,PeoplePartition,ContactPartition,AddressPartition broker
    class PeopleConsumer,ContactConsumer,AddressConsumer,PeopleWorker,ContactWorker,AddressWorker consumer
    class PersonWorkflow,ValidationSteps,DB processing
```

---

## 12. Dependency Injection Container

```mermaid
graph TB
    subgraph "WebAPI - Program.cs"
        WebApiServices["Service Registration"]

        WebApiServices -->|AddDbContextPool| PeopleContext["PeopleContext&lt;PeopleContext&gt;<br/>EF Core DbContext"]
        WebApiServices -->|AddKafkaProducer| ProducerPerson["IProducer&lt;int, Person&gt;"]
        WebApiServices -->|AddKafkaProducer| ProducerContact["IProducer&lt;int, ContactInfo&gt;"]
        WebApiServices -->|AddKafkaProducer| ProducerAddress["IProducer&lt;int, Address&gt;"]
        WebApiServices -->|AddControllers| Controllers["PeopleController<br/>ContactController<br/>AddressController"]
        WebApiServices -->|AddOpenApi| OpenApi["OpenAPI + Scalar UI"]
        WebApiServices -->|AddServiceDefaults| Telemetry["Logging & Telemetry"]
    end

    subgraph "Consumer - Program.cs"
        ConsumerServices["Service Registration"]

        ConsumerServices -->|AddDbContextPool| PeopleContext2["PeopleContext&lt;PeopleContext&gt;<br/>EF Core DbContext"]
        ConsumerServices -->|AddKafkaConsumer| ConsumerPerson["IConsumer&lt;int, Person&gt;<br/>people-topic"]
        ConsumerServices -->|AddKafkaConsumer| ConsumerContact["IConsumer&lt;int, ContactInfo&gt;<br/>contact-topic"]
        ConsumerServices -->|AddKafkaConsumer| ConsumerAddress["IConsumer&lt;int, Address&gt;<br/>address-topic"]
        ConsumerServices -->|AddHostedService| PeopleWorker["PeopleWorker<br/>BackgroundService"]
        ConsumerServices -->|AddHostedService| ContactWorker["ContactWorker<br/>BackgroundService"]
        ConsumerServices -->|AddHostedService| AddressWorker["AddressWorker<br/>BackgroundService"]
        ConsumerServices -->|AddWorkflow| Workflow["PersonWorkflow&lt;int, PersonState&gt;"]
        ConsumerServices -->|RegisterStep| Step1["ValidatePersonStep"]
        ConsumerServices -->|RegisterStep| Step2["ValidateContactStep"]
        ConsumerServices -->|RegisterStep| Step3["ValidateAddressStep"]
        ConsumerServices -->|AddServiceDefaults| Telemetry2["Logging & Telemetry"]
    end

    subgraph "ServiceDefaults - Extensions.cs"
        Extensions["AddServiceDefaults&lt;TBuilder&gt;"]
        Extensions -->|ConfigureOpenTelemetry| Logging["Serilog + OpenTelemetry"]
        Extensions -->|AddDefaultHealthChecks| HealthDb["Database Health Check"]
        Extensions -->|AddDefaultHealthChecks| HealthKafka["Kafka Health Check"]
        Extensions -->|AddServiceDiscovery| Discovery["Service Discovery"]
        Extensions -->|AddResilienceHandler| Resilience["Resilience & Retry Logic"]
    end

    subgraph "Serialization"
        Serializations["KafkaJsonSerializer&lt;T&gt;"]
        Deserialization["KafkaJsonDeserializer&lt;T&gt;"]
        Serializations -->|Uses| JsonSettings["JsonSerializerSettings<br/>CamelCase Naming<br/>Ignore Null Values<br/>ISO DateTime"]
    end

    WebApiServices -->|Uses| Extensions
    ConsumerServices -->|Uses| Extensions

    ProducerPerson -.->|Uses| Serialization
    ProducerContact -.->|Uses| Serialization
    ProducerAddress -.->|Uses| Serialization

    ConsumerPerson -.->|Uses| Deserialization
    ConsumerContact -.->|Uses| Deserialization
    ConsumerAddress -.->|Uses| Deserialization

    classDef registration fill:#3498db,stroke:#2c3e50,color:#fff
    classDef service fill:#2ecc71,stroke:#2c3e50,color:#fff
    classDef extension fill:#9b59b6,stroke:#2c3e50,color:#fff
    classDef serialization fill:#f39c12,stroke:#2c3e50,color:#fff

    class WebApiServices,ConsumerServices registration
    class PeopleContext,ProducerPerson,ProducerContact,ProducerAddress,ConsumerPerson,ConsumerContact,ConsumerAddress,PeopleWorker,ContactWorker,AddressWorker,Workflow,Step1,Step2,Step3 service
    class Extensions,Logging,HealthDb,HealthKafka,Discovery,Resilience extension
    class Serialization,Deserialization,JsonSettings serialization
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

These diagrams provide comprehensive visualization of the **KafkaWorkflow** distributed system architecture:

1. **System Architecture Overview** - Complete component layout with layers
2. **Data Flow Paths** - Synchronous and asynchronous request processing
3. **Service Dependencies** - How services reference and orchestrate each other
4. **Workflow Pipeline** - Step-by-step workflow execution and state management
5. **Database Schema** - Entity relationships (Person → ContactInfo → Address)
6. **Class Hierarchy** - OOP structure and inheritance of workflow framework
7. **REST API Endpoints** - All controller endpoints with HTTP methods and query parameters
8. **Testing Pyramid** - Test organization by layer (Unit → Integration → E2E)
9. **Aspire Dashboard** - Monitoring, resources, logs, and traces
10. **Complete Sequence Flow** - Full request lifecycle from POST to workflow completion
11. **Kafka Message Flow** - Three separate topics (people, contact, address) with workers
12. **Dependency Injection** - Service registration and configuration setup
13. **State Management** - Workflow state transitions and validation pipeline
14. **Project Dependencies** - Project references and relationships
15. **Error Handling** - Exception handling and recovery mechanisms

### Key Architecture Features

- **Three-Topic Kafka Architecture**: Separate topics for `people-topic`, `contact-topic`, and `address-topic`
- **Three-Worker Pattern**: `PeopleWorker`, `ContactWorker`, and `AddressWorker` process events independently
- **Unified Workflow**: All three workers execute the same `PersonWorkflow` orchestrator
- **Route-Based Parameters**: POST endpoints use route parameters (`/contact/{personId}`, `/address/{contactInfoId}`)
- **Generic Serialization**: Reusable `KafkaJsonSerializer<T>` and `KafkaJsonDeserializer<T>` for type safety
- **Three-Step Validation**: Sequential validation (Person → Contact → Address) in each workflow execution

All diagrams are created using **Mermaid** syntax and can be rendered in:
- GitHub markdown files
- GitLab markdown files
- Live Editor (https://mermaid.live)
- VS Code with Markdown Preview Support
- Any markdown renderer with support
