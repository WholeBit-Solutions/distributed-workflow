# Playwright Tests on Aspire Dashboard - Quick Start

## TL;DR - Get Tests Running in 2 Commands

```bash
# 1. Start Aspire Dashboard
dotnet run --project KafkaWorkflow.AppHost

# 2. Open Dashboard (usually at http://localhost:18626)
# Then click "Run" on the "playwright" resource
```

## What Changed?

| Component | Change | Why |
|-----------|--------|-----|
| **PlaywrightFixture.cs** | Added environment variable support + health checks | Works with Aspire's dynamic service endpoints |
| **AppHost.cs** | Enabled Playwright project resource | Tests can be triggered from dashboard |
| **AppHost.csproj** | Added Playwright project reference | Required for Aspire to discover the project |

## How Aspire Integration Works

```
Aspire Dashboard UI
        ↓ (click Run)
AppHost orchestrates services
        ↓
Starts: SQL Server → Kafka → WebAPI
        ↓
Passes SERVICES__WEBAPI__HTTP__0 to tests
        ↓
PlaywrightFixture reads environment variable
        ↓
Waits for service health check
        ↓
Launches browser + runs tests
        ↓
Results shown in dashboard logs
```

## Key Features

- **Automatic Discovery**: Tests find API endpoint from Aspire
- **Health Checks**: Retries 30 times (30 seconds) if service is starting
- **Self-Signed Certs**: Automatically trusted in development
- **Dependency Management**: SQL + Kafka start before API + tests
- **Explicit Start**: Tests only run when you click Run in dashboard

## Port Configuration

Aspire automatically assigns ports. To find the actual port:

1. Open Aspire Dashboard
2. Check Resources → "webapi" → View Details
3. Look for HTTP/HTTPS port
4. Environment variable `SERVICES__WEBAPI__HTTP__0` is set automatically

## Testing Without Aspire

```bash
# Terminal 1: Start API manually
dotnet run --project KafkaWorkflow.WebApi

# Terminal 2: Run tests
dotnet test test/KafkaWorkflow.PlaywrightTests/
```

Or with custom URL:
```bash
$env:PLAYWRIGHT_BASE_URL = "https://localhost:7500"
dotnet test test/KafkaWorkflow.PlaywrightTests/
```

## Monitoring Test Execution

1. **Dashboard Logs**: Click resource → View Details → Logs tab
2. **Real-time**: Watch as tests run with Playwright taking screenshots
3. **Status**: Green checkmark when complete, red X on failure

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Tests not appearing | Rebuild solution, refresh dashboard |
| "Service not available" | Check WebAPI status in dashboard |
| SSL errors | Already handled (self-signed OK) |
| Port already in use | Let Aspire assign alternate port |
| Browser not installing | Run `dotnet test` once to auto-install |

## Environment Variables (For Reference)

| Variable | Set By | Used For |
|----------|--------|----------|
| `SERVICES__WEBAPI__HTTP__0` | Aspire | Primary endpoint source |
| `PLAYWRIGHT_BASE_URL` | Manual | Override default URL |
| `ASPIRE` | AppHost.cs | Identify Aspire environment |

## Documentation Files

- **ASPIRE_SETUP.md** - Detailed setup & configuration guide
- **ASPIRE_INTEGRATION_SUMMARY.md** - Technical overview of changes
- **This file** - Quick reference cheat sheet

---

**Ready to go!** Start with: `dotnet run --project KafkaWorkflow.AppHost`
