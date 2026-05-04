# Architecture Documentation Index

This guide helps you navigate the architecture documentation for the KafkaWorkflow system.

---

## 📋 Two Main Architecture Documents

### 1. **ARCHITECTURE_OVERVIEW.md** 📄
**Comprehensive written architecture documentation**

Contains:
- System overview and high-level architecture
- Core components explanation
- Projects and responsibilities
- Request flow examples
- Workflow architecture details
- Entity relationships
- Data flow diagrams
- Services and ports
- Testing layers
- Aspire orchestration setup
- Key interfaces
- Running instructions
- Key files reference

**Best for:**
- Understanding the overall system design
- Learning how components interact
- Development and debugging
- Onboarding new team members
- API documentation

**Start here:** Go to [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md)

---

### 2. **ARCHITECTURE_DIAGRAMS.md** 📊
**Visual diagrams showing system architecture**

Contains 15 comprehensive diagrams:
1. System Architecture Overview
2. Component Interaction & Data Flow
3. Service Dependencies & Wiring
4. Workflow Execution Pipeline
5. Database Entity Relationships
6. Class Hierarchy
7. REST API Endpoints
8. Testing Pyramid
9. Aspire Dashboard Resources
10. Complete System Flow Sequence (25 steps)
11. Kafka Message Flow (Three Topics)
12. Dependency Injection Container
13. State Management in Workflow
14. Project Dependencies Map
15. Error Handling Flow

**Best for:**
- Visual learners
- Understanding architecture at a glance
- Presentations
- Documentation
- Quick reference

**Start here:** Go to [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md)

---

## 🎯 Quick Navigation by Goal

### "I want to understand the system quickly"
1. Read: [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md) - High-Level Architecture section
2. View: [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) - Diagram 1 & 10

### "I want to understand how data flows"
1. Read: [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md) - Data Flow Diagram section
2. View: [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) - Diagrams 2, 11

### "I want to understand the workflow"
1. Read: [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md) - Workflow Architecture section
2. View: [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) - Diagrams 4, 13

### "I want to understand Kafka"
1. Read: [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md) - Core Components section
2. View: [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) - Diagram 11

### "I want to understand the database"
1. Read: [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md) - Entity Relationships section
2. View: [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) - Diagram 5

### "I want to understand testing"
1. Read: [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md) - Testing Layers section
2. View: [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) - Diagram 8

### "I want to understand Aspire"
1. Read: [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md) - Aspire Orchestration section
2. View: [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) - Diagrams 3, 9

### "I want to understand dependency injection"
1. Read: [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md) - Key Interfaces section
2. View: [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) - Diagram 12

### "I want to see all endpoints"
1. Read: [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md) - Core Components (WebAPI Service)
2. View: [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) - Diagram 7

---

## 📚 Related Documentation

| Document | Purpose |
|----------|---------|
| **README.md** | Project overview, getting started, key features |
| **ARCHITECTURE_OVERVIEW.md** | Detailed architecture and design (THIS FILE) |
| **ARCHITECTURE_DIAGRAMS.md** | Visual diagrams (15 comprehensive diagrams) |
| **test/KafkaWorkflow.PlaywrightTests/ASPIRE_SETUP.md** | Test setup and troubleshooting |
| **CrossControllerIntegration.http** | Manual HTTP testing file |

---

## 🔍 Diagram Directory

### Basic Concepts (Start Here)
- **Diagram 1**: System Architecture Overview - The big picture
- **Diagram 5**: Database Entity Relationships - Data model
- **Diagram 7**: REST API Endpoints - All endpoints listed

### Data & Flow
- **Diagram 2**: Component Interaction & Data Flow - Sync vs async
- **Diagram 10**: Complete System Flow Sequence - 25-step walkthrough
- **Diagram 11**: Kafka Message Flow - Three-topic architecture

### Design & Structure
- **Diagram 3**: Service Dependencies & Wiring - Service relationships
- **Diagram 6**: Class Hierarchy - Workflow framework
- **Diagram 14**: Project Dependencies Map - Project organization

### Operations & Orchestration
- **Diagram 4**: Workflow Execution Pipeline - Step-by-step workflow
- **Diagram 9**: Aspire Dashboard Resources - Container management
- **Diagram 12**: Dependency Injection Container - Service registration

### Advanced Topics
- **Diagram 8**: Testing Pyramid - Test organization
- **Diagram 13**: State Management in Workflow - State transitions
- **Diagram 15**: Error Handling Flow - Exception handling

---

## 💡 Reading Recommendations

### For Project Managers
1. ARCHITECTURE_OVERVIEW.md - High-Level Architecture
2. ARCHITECTURE_DIAGRAMS.md - Diagrams 1, 8
3. README.md - Key Features

### For Developers (Getting Started)
1. README.md - Getting Started section
2. ARCHITECTURE_OVERVIEW.md - Complete read
3. ARCHITECTURE_DIAGRAMS.md - Diagrams 1, 10, 4
4. test/KafkaWorkflow.PlaywrightTests/ASPIRE_SETUP.md

### For Architects
1. ARCHITECTURE_OVERVIEW.md - Complete read
2. ARCHITECTURE_DIAGRAMS.md - All diagrams

### For QA/Testing
1. ARCHITECTURE_OVERVIEW.md - Testing Layers section
2. ARCHITECTURE_DIAGRAMS.md - Diagrams 8, 10
3. test/KafkaWorkflow.PlaywrightTests/ASPIRE_SETUP.md

### For DevOps
1. ARCHITECTURE_OVERVIEW.md - Aspire Orchestration section
2. ARCHITECTURE_DIAGRAMS.md - Diagrams 3, 9, 12
3. KafkaWorkflow.AppHost/AppHost.cs

---

## ✨ Key Highlights

### Three-Topic Kafka Architecture
- `people-topic` → PeopleWorker
- `contact-topic` → ContactWorker
- `address-topic` → AddressWorker

All three workers execute **PersonWorkflow** with **3-step validation**

### REST Endpoints with Route Parameters
- `POST /contact/{personId}` - Create contact with personId as route parameter
- `POST /address/{contactInfoId}` - Create address with contactInfoId as route parameter

### Unified Workflow Orchestration
- Generic framework: `BusinessWorkflow<int, PersonState>`
- Pluggable steps: `IMessageWorkflowStep<int, PersonState>`
- Sequential execution with error handling

### Complete Testing Coverage
- **Unit Tests**: Workflow + Steps (NUnit + Moq)
- **Integration Tests**: Controllers + Full flow (Playwright C#)

---

## 🚀 Getting Started

### Option 1: Quick Start (30 minutes)
1. Read ARCHITECTURE_OVERVIEW.md - High-Level Architecture section
2. View ARCHITECTURE_DIAGRAMS.md - Diagram 1
3. View ARCHITECTURE_DIAGRAMS.md - Diagram 10
4. Run: `dotnet run --project KafkaWorkflow.AppHost`

### Option 2: Complete Understanding (2-3 hours)
1. Read README.md - Complete
2. Read ARCHITECTURE_OVERVIEW.md - Complete
3. View ARCHITECTURE_DIAGRAMS.md - All diagrams
4. Read test/KafkaWorkflow.PlaywrightTests/ASPIRE_SETUP.md
5. Run and explore the system

### Option 3: Deep Dive (4-6 hours)
1. Complete Option 2
2. Read DIAGRAMS_NAVIGATION.md - Complete
3. Explore the codebase:
   - KafkaWorkflow.WebApi/Program.cs
   - KafkaWorkflow.Consumer/Program.cs
   - KafkaWorkflow.Consumer/PeopleWorkflow/PersonWorkflow.cs
4. Review test files

---

## 📊 Diagram Legend

### Colors
- 🔵 **Blue**: APIs, Controllers, Services
- 🔴 **Red**: Errors, Failures, Async Processing
- 🟢 **Green**: Success, Database, Storage
- 🟠 **Orange**: Infrastructure, Configuration, Orchestration
- 🟣 **Purple**: Testing, Logging, Utilities
- 🟦 **Cyan**: Observability, Monitoring, Dashboards

### Shapes
- **Rectangles**: Components, Services
- **Diamonds**: Decisions
- **Cylinders**: Databases
- **Circles**: Processes
- **Hexagons**: External Services

---

## 🔗 Cross-References

**ARCHITECTURE_OVERVIEW.md** references:
- Specific diagrams in ARCHITECTURE_DIAGRAMS.md
- Related documentation files
- Code files in the repository

**ARCHITECTURE_DIAGRAMS.md** provides:
- Visual representation of concepts from ARCHITECTURE_OVERVIEW.md
- Detailed flow diagrams
- Class hierarchy diagrams

**DIAGRAMS_NAVIGATION.md** helps:
- Find specific diagrams
- Understand diagram relationships
- Navigate by role (Developer, Architect, QA, etc.)

---

## ✅ Verification

All documentation is:
- ✅ Current and accurate
- ✅ Consistent across all files
- ✅ Aligned with the codebase
- ✅ Complete and comprehensive
- ✅ Easy to navigate

**Last Updated**: 2024  
**Version**: 2.0 (Three-Topic Architecture)  
**Status**: Complete & Production-Ready

---

## 🎓 Learning Path

1. **Week 1: Understand the System**
   - Read ARCHITECTURE_OVERVIEW.md
   - View Diagrams 1, 5, 7
   - Run the application
   - Explore Aspire Dashboard

2. **Week 2: Understand the Data Flow**
   - View Diagrams 2, 10, 11
   - Study Entity Relationships
   - Trace a request through the system

3. **Week 3: Understand the Workflow**
   - Read Workflow Architecture
   - View Diagrams 4, 6, 13
   - Study the PersonWorkflow code
   - Write a test

4. **Week 4: Understand Everything**
   - View remaining diagrams
   - Review all code files
   - Contribute to the project

---

## 📞 Questions?

Refer to the appropriate documentation:
- **"What is this component?"** → ARCHITECTURE_OVERVIEW.md
- **"How does X work visually?"** → ARCHITECTURE_DIAGRAMS.md
- **"Which diagram shows X?"** → DIAGRAMS_NAVIGATION.md
- **"How do I run tests?"** → test/KafkaWorkflow.PlaywrightTests/ASPIRE_SETUP.md
- **"How do I get started?"** → README.md

---

**Start exploring**: [ARCHITECTURE_OVERVIEW.md](./ARCHITECTURE_OVERVIEW.md) or [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md)
