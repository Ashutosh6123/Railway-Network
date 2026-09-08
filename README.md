# Railway Reservation System

This repository contains the Phase 0 solution skeleton for the Railway Reservation System.

## Prerequisites

- .NET SDK 10.0.400 or a compatible .NET 10 SDK
- SQL Server 2019 for later database-owning phases

## Solution layout

- `src/` contains the seven HTTP applications.
- `tests/` contains NUnit test-project skeletons for the five services with planned application logic.
- `docs/` is reserved for project documentation.

## Local HTTP ports

| Application | Port |
| --- | ---: |
| API Gateway | 5000 |
| User Service | 5001 |
| Train Service | 5002 |
| Reservation Service | 5003 |
| Payment Service | 5004 |
| Mail Service | 5005 |
| Dummy Razorpay Gateway | 5006 |

## Build and test

```powershell
dotnet restore RailwayNetwork.sln
dotnet build RailwayNetwork.sln
dotnet test RailwayNetwork.sln
```

Phase 0 deliberately contains no business endpoints, database contexts, migrations, or service workflows. Those are implemented only in their assigned later phases.
