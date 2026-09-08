# Railway Reservation System
# AI Implementation Guide

**Document:** AI Implementation Guide  
**Project:** Railway Reservation System  
**Version:** 1.0  
**Status:** Implementation Baseline  
**Primary Design Reference:** Railway Reservation System — Low Level Design (LLD), Version 1.5  
**Technology:** C# / .NET / ASP.NET Core / Entity Framework Core / SQL Server  
**Architecture:** Microservices + Layered Application Architecture  
**Testing Framework:** NUnit  

---

# 1. Purpose and Scope

## 1.1 Purpose

This document is the implementation guide for AI coding agents working on the Railway Reservation System.

Its purpose is to translate the finalized Low Level Design (LLD) into a structured, incremental, and verifiable implementation plan.

The coding agent MUST treat the LLD as the primary design authority and this document as the execution guide.

The agent is expected to:

- implement the system incrementally;
- preserve the approved architecture;
- preserve service and database ownership boundaries;
- implement the approved domain model;
- implement authentication and authorization;
- implement the booking, cancellation, payment, refund, seat-allocation, and waitlist rules;
- create automated tests;
- compile and verify changes after meaningful implementation steps;
- avoid unnecessary architectural complexity.

## 1.2 Scope

The implementation covers:

1. API Gateway
2. User Service
3. Train Service
4. Reservation Service
5. Payment Service
6. Mail Service
7. Dummy Razorpay Gateway

It also covers:

- SQL Server databases;
- Entity Framework Core;
- JWT authentication;
- role-based authorization;
- HTTP/REST inter-service communication;
- SMTP email delivery;
- payment simulation;
- train/station/route/coach/seat/fare management;
- ticket booking;
- cancellation and refund;
- dynamic segment-based seat availability;
- FIFO waitlist management;
- waitlist promotion;
- error handling and logging;
- configuration and secrets;
- unit, integration, API, and end-to-end testing.

## 1.3 Implementation Philosophy

The implementation MUST be incremental.

Each phase should leave the solution in a buildable and testable state.

The agent MUST prefer straightforward, maintainable C#/.NET implementation over sophisticated abstractions.

The guide is NOT permission to redesign the system.

---

# 2. Source of Truth, Design Authority and Conflict Resolution

## 2.1 Primary Authority

The finalized Railway Reservation System LLD, Version 1.5, is the primary design authority.

The LLD defines WHAT the system must contain and how it behaves.

This implementation guide defines HOW and WHEN the approved design should be implemented.

## 2.2 No Silent Design Changes

The agent MUST NOT silently:

- add new microservices;
- change service ownership;
- introduce shared databases;
- add entities that materially change the domain;
- change API contracts;
- change authentication rules;
- change booking rules;
- change waitlist behavior;
- replace SQL Server;
- replace .NET;
- replace the payment simulation;
- introduce infrastructure such as Kafka, Redis, Kubernetes, or service discovery.

If a genuine conflict exists, the agent MUST identify it before making the change.

## 2.3 Conflict Resolution

Use the following hierarchy:

1. Approved requirements
2. Finalized LLD
3. This implementation guide
4. Existing implementation that is demonstrably consistent with the above
5. Simplest implementation compatible with the above

If the conflict cannot be resolved safely, stop and request clarification.

## 2.4 Baseline That Must Be Preserved

The implementation MUST preserve:

- six core services;
- Dummy Razorpay Gateway as the external payment simulator;
- separate databases for User, Train, Reservation, and Payment;
- JWT authentication;
- Passenger and Administrator roles;
- Reservation as one microservice;
- segment-based availability;
- atomic confirmed/waitlisted booking;
- FIFO waitlist;
- payment-before-final-booking-state;
- compensating refund when persistence fails after successful payment;
- registered user email as the booking notification address;
- no raw card/CVV/bank credential storage;
- no unnecessary infrastructure.

---

# 3. AI Agent Operating Rules

The AI agent is an implementation engineer, not an independent architect.

## 3.1 Required Working Method

Before modifying code:

1. inspect the repository;
2. understand the existing implementation;
3. identify the relevant service/project;
4. compare existing code with the LLD;
5. make a small implementation plan;
6. implement the smallest correct change;
7. build;
8. run relevant tests;
9. fix failures;
10. review against the LLD;
11. report what changed;
12. continue only after the current phase is stable.

## 3.2 Layering

Follow:

```text
Controller
    ↓
Application Service
    ↓
Repository Interface
    ↓
Repository
    ↓
EF Core DbContext
    ↓
SQL Server
```

For external services:

```text
Application Service
    ↓
Client Interface
    ↓
Client Implementation
    ↓
HTTP
    ↓
Remote Service
```

## 3.3 Do Not Overengineer

Do NOT introduce unless explicitly approved:

- Kafka;
- RabbitMQ;
- Redis;
- Kubernetes;
- service discovery;
- CQRS;
- event sourcing;
- Saga frameworks;
- distributed transactions;
- distributed locks;
- unnecessary caching;
- unnecessary background processing;
- generic repository frameworks;
- excessive design patterns;
- additional microservices.

### 3.3.1 Code Simplicity and Learnability

This project is intended to be educational as well as functional. The most important goal is that a developer who understands basic C# and .NET can read, follow, and understand the complete project.

The AI agent MUST prefer simple, explicit, readable code over clever, highly abstract, or unnecessarily optimized code. The simplest correct implementation should be preferred. Do NOT optimize for code brevity; optimize for readability, maintainability, and understandability.

Prefer:

- straightforward classes and methods;
- descriptive names;
- small methods with one clear responsibility;
- normal `if`/`else` and `switch` statements when they make logic easier to follow;
- simple loops where they are clearer than complex LINQ;
- basic LINQ only when it genuinely improves readability;
- conventional dependency injection;
- simple repository and application-service classes;
- explicit DTO mapping;
- clear EF Core queries;
- clear validation and exception handling;
- comments that explain non-obvious business rules or WHY a particular step is required.

Avoid unless the LLD or a concrete requirement clearly requires them:

- complex LINQ chains that are difficult to read;
- reflection;
- expression trees;
- `dynamic` typing;
- advanced generic abstractions;
- custom framework or abstraction layers;
- complicated combinations of design patterns;
- CQRS/MediatR-style abstractions;
- event sourcing;
- specification-pattern frameworks;
- generic repositories that hide simple data access;
- unnecessary factory/strategy hierarchies;
- source generators;
- large extension-method frameworks;
- complicated async/concurrency abstractions;
- clever one-line implementations that make business logic harder to understand.

The following are acceptable and should be used when required by the approved design: `async`/`await`, dependency injection, interfaces defined by the LLD, EF Core, JWT authentication, `HttpClient`, database transactions, SQL isolation/locking required for seat concurrency, basic LINQ, password hashing, logging, and DTOs. These technologies should still be used in their simplest understandable form.

Before introducing an unfamiliar or advanced technique, the agent MUST first consider whether the same requirement can be implemented clearly with basic C#/.NET features. If a simpler approach satisfies the requirement, use the simpler approach.

Do not add abstractions merely to make the code look more architecturally sophisticated. Every abstraction should have a clear reason grounded in the LLD or a concrete implementation requirement.

## 3.4 Service Ownership

A service MUST access only its own database.

There must be:

- no cross-service database access;
- no cross-service EF navigation properties;
- no cross-service EF foreign keys;
- no shared DbContext;
- no shared database.

Reservation communicates with User, Train, Payment, and Mail through HTTP client abstractions.

## 3.5 Security Rules

The agent MUST:

- hash passwords;
- validate JWTs;
- enforce roles;
- enforce booking ownership;
- keep secrets outside source code;
- never store raw card numbers, CVV, or bank credentials;
- never bypass authentication merely to make a test pass.

## 3.6 Completion Rules

Completed functionality MUST NOT contain:

- TODO placeholders;
- FIXME placeholders;
- `NotImplementedException`;
- fake success responses;
- swallowed exceptions;
- incomplete core workflows.

---

# 4. Project Overview

The system is an online railway reservation platform.

Passengers can:

- register and log in;
- search trains;
- view train details;
- view schedules;
- view fares;
- check actual accommodation availability;
- book tickets;
- view reservations;
- cancel reservations.

Administrators can manage railway master data.

The architecture contains:

```text
Client Application
       ↓
API Gateway
       ↓
 ┌─────┼───────────────┐
 ↓     ↓               ↓
User  Train       Reservation
Service Service       Service
                         ↓
                ┌────────┼────────┐
                ↓        ↓        ↓
              User     Train    Payment
                                  ↓
                            Dummy Razorpay

Reservation → Mail Service → SMTP
```

Each core data-owning service has its own database:

```text
User Service         → User DB
Train Service        → Train DB
Reservation Service  → Reservation DB
Payment Service      → Payment DB
```

Mail Service, API Gateway, and Dummy Razorpay Gateway do not own databases.

## 4.1 Segment-Based Availability

Availability is calculated dynamically.

For an existing allocation:

```text
existingFromOrder < requestedToOrder
AND
requestedFromOrder < existingToOrder
```

If both conditions are true, the journeys overlap.

Example:

```text
A → C
```

can reuse the same seat for:

```text
C → E
```

because they do not overlap.

But:

```text
A → C
```

conflicts with:

```text
B → D
```

because the segments overlap.

## 4.2 Atomic Booking

A booking containing 1–6 passengers is atomic.

If enough seats exist for every passenger:

```text
Confirmed
```

Otherwise:

```text
Waitlisted
```

There is no partial confirmation and no RAC state.

## 4.3 Waitlist

Waitlist order is strict FIFO.

A waiting booking can be promoted only if enough seats are available for the entire booking.

No waiting booking may be skipped merely because a later booking requires fewer seats.

Waitlisted bookings are eligible for promotion only until the journey starts.

---

# 5. Technology Stack

## 5.1 Technology Baseline

| Area | Technology |
|---|---|
| Programming Language | C# |
| .NET SDK | .NET 10 SDK |
| Runtime / Framework | .NET 10 / ASP.NET Core 10 |
| API Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Database Engine | Microsoft SQL Server 2019 |
| Database Management Tool | SQL Server Management Studio (SSMS) 22 |
| Authentication | JWT Bearer Authentication |
| Authorization | Role-Based Authorization |
| Inter-Service Communication | HTTP / REST |
| Email Delivery | SMTP |
| Payment Simulation | Dummy Razorpay Gateway |
| Unit Testing | NUnit |
| Dependency Injection | ASP.NET Core built-in Dependency Injection |
| Configuration | ASP.NET Core configuration system |
| Logging | ASP.NET Core logging / `ILogger<T>` |

The implementation MUST target .NET 10.

The database implementation MUST be compatible with SQL Server 2019.

SSMS 22 is the primary database management and inspection tool during development.

The repository SHOULD include `global.json` to pin the intended .NET 10 SDK version.

The agent MUST NOT silently target a different major .NET version or replace SQL Server 2019.

## 5.2 Technology Constraints

Major technology replacements require explicit approval.

Do not replace:

- .NET 10;
- ASP.NET Core;
- EF Core;
- SQL Server 2019;
- JWT;
- HTTP/REST;
- SMTP;
- NUnit.

---

# 6. Final Architecture Contract

## 6.1 Core Applications

The system contains exactly six core applications/services:

1. API Gateway
2. User Service
3. Train Service
4. Reservation Service
5. Payment Service
6. Mail Service

External dependency:

7. Dummy Razorpay Gateway

## 6.2 Communication

```text
Passenger / Administrator
          ↓
   Client Application
          ↓
     API Gateway
      ↓   ↓   ↓
    User Train Reservation
                ↓
        ┌───────┼────────┐
        ↓       ↓        ↓
      User    Train    Payment
                          ↓
                    Dummy Razorpay

Reservation → Mail Service → SMTP
```

The Gateway routes only to:

- User Service;
- Train Service;
- Reservation Service.

The Gateway MUST NOT directly route client requests to:

- Payment Service;
- Mail Service;
- Dummy Razorpay Gateway.

## 6.3 Gateway Responsibility

The API Gateway performs:

- routing;
- JWT validation;
- role-based authorization.

It does NOT contain:

- business logic;
- database access;
- reservation logic;
- payment logic;
- mail logic;
- load balancing;
- rate limiting.

## 6.4 Reservation Boundary

Reservation remains one microservice.

The following are internal components/services:

- Booking;
- Availability;
- Waitlist.

They are NOT separate deployable microservices.

## 6.5 Database Ownership

```text
User Service        → User DB
Train Service       → Train DB
Reservation Service → Reservation DB
Payment Service     → Payment DB
Mail Service        → No DB
API Gateway         → No DB
Dummy Razorpay      → No DB
```

No shared database is allowed.

---

# 7. Service Boundaries and Responsibilities

## 7.1 API Gateway

Responsibilities:

- client entry point;
- routing;
- JWT validation;
- authorization.

No business logic.

## 7.2 User Service

Responsibilities:

- registration;
- login;
- password hashing;
- JWT generation;
- role lookup;
- user lookup;
- User DB ownership.

## 7.3 Train Service

Responsibilities:

- train information;
- station information;
- route stops and schedules;
- coach information;
- seat master data;
- fare management;
- train search;
- public train information;
- administrator master-data management.

Train Service is authoritative for current fares.

## 7.4 Reservation Service

Responsibilities:

- ticket booking;
- reservation retrieval;
- cancellation;
- actual seat availability;
- seat allocation;
- waitlist;
- waitlist promotion.

Reservation Service owns Reservation DB.

## 7.5 Payment Service

Responsibilities:

- payment processing;
- refund processing;
- Payment DB;
- communication with Dummy Razorpay Gateway.

Payment Service MUST NOT contain reservation business logic.

## 7.6 Mail Service

Responsibilities:

- notification orchestration;
- SMTP delivery;
- notification templates.

No database is required in the current scope.

## 7.7 Dummy Razorpay Gateway

Responsibilities:

- simulate payment success/failure;
- simulate refund;
- expose simple HTTP APIs;
- remain stateless.

Payment Service is the only application that calls Dummy Razorpay.

---

# 8. Repository Structure

Recommended structure:

```text
RailwayReservationSystem/
├── RailwayReservationSystem.sln
├── global.json
├── README.md
├── .gitignore
├── .editorconfig
├── src/
│   ├── ApiGateway/
│   ├── UserService/
│   ├── TrainService/
│   ├── ReservationService/
│   ├── PaymentService/
│   ├── MailService/
│   └── DummyRazorpayGateway/
├── tests/
│   ├── UserService.Tests/
│   ├── TrainService.Tests/
│   ├── ReservationService.Tests/
│   ├── PaymentService.Tests/
│   └── MailService.Tests/
└── docs/
```

## 8.1 User Service

```text
UserService/
├── Controllers/
├── Services/
├── Repositories/
├── Entities/
├── DTOs/
├── Data/
├── Exceptions/
├── Middleware/
├── Program.cs
└── appsettings.json
```

## 8.2 Train Service

```text
TrainService/
├── Controllers/
├── Services/
├── Repositories/
├── Entities/
├── DTOs/
├── Data/
├── Enums/
├── Exceptions/
├── Middleware/
├── Program.cs
└── appsettings.json
```

## 8.3 Reservation Service

```text
ReservationService/
├── Controllers/
├── Services/
├── Repositories/
├── Clients/
├── Entities/
├── DTOs/
├── Enums/
├── Data/
├── Exceptions/
├── Middleware/
├── Program.cs
└── appsettings.json
```

Reservation contains:

- `IBookingService` / `BookingService`
- `IAvailabilityService` / `AvailabilityService`
- `IWaitlistService` / `WaitlistService`
- `IBookingRepository`
- `IBookingPassengerRepository`
- `ISeatAllocationRepository`
- `IWaitlistRepository`
- `IUserClient`
- `ITrainClient`
- `IPaymentClient`
- `IMailClient`

## 8.4 Payment Service

```text
PaymentService/
├── Controllers/
├── Services/
├── Repositories/
├── Gateways/
├── Entities/
├── DTOs/
├── Enums/
├── Data/
├── Exceptions/
├── Middleware/
├── Program.cs
└── appsettings.json
```

## 8.5 Mail Service

```text
MailService/
├── Controllers/
├── IMailService
├── MailService
├── Email/
├── DTOs/
├── Templates/
├── Exceptions/
├── Middleware/
├── Program.cs
└── appsettings.json
```

No DbContext is required.

## 8.6 Dummy Razorpay Gateway

Keep this application small.

It should contain:

- controller;
- request/response DTOs;
- payment/refund simulation logic;
- configuration for deterministic test behavior.

No database or repository is required.

## 8.7 Dependency Rules

Each service is self-contained.

Reservation communicates with other services through HTTP clients.

No project should reference another service's EF entities.

---

# 9. Domain Model Contract

## 9.1 User

```text
User
- Id : int
- Name : string
- Email : string
- PhoneNumber : string
- PasswordHash : string
- RoleId : int
- CreatedAt : DateTime
```

## 9.2 Role

```text
Role
- Id : int
- Name : string
```

Roles:

```text
Passenger
Administrator
```

Role is a database entity, not an enum.

Normal registration always assigns Passenger.

Administrator accounts are provisioned/seeded through controlled means.

## 9.3 Train

```text
Train
- Id : int
- TrainNumber : string
- Name : string
```

## 9.4 Station

```text
Station
- Id : int
- Code : string
- Name : string
```

## 9.5 RouteStop

```text
RouteStop
- Id : int
- TrainId : int
- StationId : int
- StopOrder : int
- ArrivalTime : TimeSpan
- DepartureTime : TimeSpan
```

There is NO separate Route entity.

The ordered collection of RouteStop records represents the route.

## 9.6 Coach

```text
Coach
- Id : int
- TrainId : int
- CoachNumber : string
- CoachType : CoachType
```

## 9.7 Seat

```text
Seat
- Id : int
- CoachId : int
- SeatNumber : string
```

There is NO `IsAvailable` property.

## 9.8 Fare

```text
Fare
- Id : int
- TrainId : int
- FromStationId : int
- ToStationId : int
- CoachType : CoachType
- Amount : decimal
```

Train Service owns current fare information.

## 9.9 Booking

```text
Booking
- Id : int
- Pnr : string
- UserId : int
- TrainId : int
- FromStationId : int
- ToStationId : int
- JourneyDate : DateTime
- CoachType : CoachType
- Quota : QuotaType
- Status : BookingStatus
- TotalFare : decimal
- CreatedAt : DateTime
- UpdatedAt : DateTime
- CancelledAt : DateTime?
```

## 9.10 BookingPassenger

```text
BookingPassenger
- Id : int
- BookingId : int
- Name : string
- Age : int
- Gender : Gender
- Address : string
```

No email is stored here.

## 9.11 SeatAllocation

```text
SeatAllocation
- Id : int
- BookingId : int
- BookingPassengerId : int
- CoachId : int
- SeatId : int
- FromStationId : int
- ToStationId : int
```

`BookingPassengerId` is mandatory so each passenger can be mapped to their allocated seat.

## 9.12 WaitlistEntry

```text
WaitlistEntry
- Id : int
- BookingId : int
- Position : int
- CreatedAt : DateTime
```

## 9.13 Payment

```text
Payment
- Id : int
- BookingId : int
- Amount : decimal
- PaymentStatus : PaymentStatus
- TransactionReference : string
- IdempotencyKey : string
- CreatedAt : DateTime
```

`IdempotencyKey` is unique in Payment DB and is separate from `TransactionReference`.

Never store raw card number, CVV, or bank credentials.

## 9.14 Enums

```text
CoachType
- General
- Sleeper
- AC3Tier
- AC2Tier
- AC1Tier

BookingStatus
- Confirmed
- Waitlisted
- Cancelled

QuotaType
- General
- Ladies

Gender
- Male
- Female

PaymentStatus
- Pending
- Successful
- Failed
- Refunded
```

## 9.15 Domain Invariants

- A booking contains 1–6 passengers.
- All passengers are confirmed together or all are waitlisted.
- A booking cannot be partially confirmed.
- Origin must occur before destination in the train route.
- Seat availability is segment-based.
- PNR is unique.
- Booking.TotalFare stores the fare actually charged.
- Adults and children pay the applicable full fare.
- Cancellation is allowed only for the owner's active booking before journey start.
- Waitlist is FIFO.
- Promotion requires enough seats for the entire booking.
- Promoted bookings do not pay again.
- No promotion occurs after journey start.
- Registered user email is used for notifications.
- Ladies quota requires every passenger to be female.
- General quota has no gender restriction.
- General and Ladies quota bookings share the same physical seat pool.
- Cancellation is allowed only until scheduled departure from the booking's FromStation.
- Waitlist positions are not renumbered; gaps are allowed.
- Coach/seat identifiers referenced by reservations must not be deleted or reused for different physical master data.

---

# 10. Database Contract

## 10.1 Databases

Exactly four application databases:

```text
User DB
Train DB
Reservation DB
Payment DB
```

No DB for:

- API Gateway;
- Mail Service;
- Dummy Razorpay Gateway.

Each owning service has one DbContext.

## 10.2 User DB

### Roles

```text
Roles
- Id INT IDENTITY PK
- Name NVARCHAR(50) NOT NULL UNIQUE
```

### Users

```text
Users
- Id INT IDENTITY PK
- Name NVARCHAR(100)
- Email NVARCHAR(100) NOT NULL UNIQUE
- PhoneNumber NVARCHAR(15) NOT NULL UNIQUE
- PasswordHash NVARCHAR(255)
- RoleId INT NOT NULL FK → Roles.Id
- CreatedAt DATETIME2
```

Seed:

```text
Passenger
Administrator
```

## 10.3 Train DB

### Trains

```text
Trains
- Id PK
- TrainNumber UNIQUE
- Name
```

### Stations

```text
Stations
- Id PK
- Code UNIQUE
- Name
```

### RouteStops

```text
RouteStops
- Id PK
- TrainId FK → Trains.Id
- StationId FK → Stations.Id
- StopOrder
- ArrivalTime
- DepartureTime
```

### Coaches

```text
Coaches
- Id PK
- TrainId FK → Trains.Id
- CoachNumber
- CoachType
```

### Seats

```text
Seats
- Id PK
- CoachId FK → Coaches.Id
- SeatNumber
```

### Fares

```text
Fares
- Id PK
- TrainId
- FromStationId
- ToStationId
- CoachType
- Amount
```

FromStationId and ToStationId are logical references to Stations in the same service database.

## 10.4 Reservation DB

### Bookings

```text
Bookings
- Id PK
- Pnr UNIQUE
- UserId logical → User DB.Users.Id
- TrainId logical → Train DB.Trains.Id
- FromStationId logical
- ToStationId logical
- JourneyDate
- CoachType
- Quota
- Status
- TotalFare
- CreatedAt
- UpdatedAt
- CancelledAt
```

### BookingPassengers

```text
BookingPassengers
- Id PK
- BookingId FK → Bookings.Id
- Name
- Age
- Gender
- Address
```

### SeatAllocations

```text
SeatAllocations
- Id PK
- BookingId FK → Bookings.Id
- BookingPassengerId FK → BookingPassengers.Id
- CoachId logical → Train DB.Coaches.Id
- SeatId logical → Train DB.Seats.Id
- FromStationId logical
- ToStationId logical
```

### WaitlistEntries

```text
WaitlistEntries
- Id PK
- BookingId FK → Bookings.Id
- Position
- CreatedAt
```

No cross-database FK constraints are created.

## 10.5 Payment DB

```text
Payments
- Id PK
- BookingId logical → Reservation DB.Bookings.Id
- Amount
- PaymentStatus
- TransactionReference
- IdempotencyKey UNIQUE
- CreatedAt
```

`IdempotencyKey` is persisted to make payment/refund operations idempotent.

No raw card/CVV/bank credentials.

## 10.6 Important Database Rules

Do not create:

- shared DBs;
- cross-service FK constraints;
- cross-service EF navigation properties;
- `Seat.IsAvailable`;
- Mail DB;
- Dummy Gateway DB;
- TrainRun/TrainInstance tables;
- Route table.

Availability is computed dynamically.

`Booking.TotalFare` is historical and must not automatically change when Train Service fare changes later.

## 10.7 Migrations

Database changes MUST follow:

```text
Entity / configuration
        ↓
Migration
        ↓
Migration review
        ↓
Apply migration
        ↓
Inspect using SSMS
        ↓
Test
```

Never silently modify production-like schema manually without documenting the change.

---

# 11. API Contract

## 11.1 User APIs

### Register

```http
POST /api/auth/register
```

Public.

Request:

```json
{
  "name": "Ali Khan",
  "email": "ali@example.com",
  "phoneNumber": "03001234567",
  "password": "Password123"
}
```

The request MUST NOT contain RoleId.

The server assigns Passenger.

### Login

```http
POST /api/auth/login
```

Returns JWT plus identity information.

### User Lookup

```http
GET /api/users/{userId}
```

Internal use by Reservation Service.

Response may include:

- Id;
- Name;
- Email;
- PhoneNumber;
- Role.

Never return PasswordHash.

## 11.2 Train APIs

Public:

```http
GET /api/trains/search
GET /api/trains/{trainId}
GET /api/trains/{trainId}/schedule
GET /api/trains/{trainId}/fare?from=&to=&coachType=
```

Actual availability is Reservation-owned:

```http
GET /api/reservations/availability
```

Train Service MUST NOT call Reservation Service merely to calculate availability.

## 11.3 Administrator APIs

Examples:

```http
POST /api/admin/trains
PUT /api/admin/trains/{id}

POST /api/admin/stations
PUT /api/admin/stations/{id}

POST /api/admin/route-stops
PUT /api/admin/route-stops/{id}

POST /api/admin/coaches
PUT /api/admin/coaches/{id}

POST /api/admin/seats
PUT /api/admin/seats/{id}

POST /api/admin/fares
PUT /api/admin/fares/{id}
```

Administrator role required.

## 11.4 Reservation APIs

### Create Booking

```http
POST /api/reservations
```

Passenger role required.

Request contains:

```text
TrainId
FromStationId
ToStationId
JourneyDate
CoachType
Quota
Passengers[]
```

Each passenger contains:

```text
Name
Age
Gender
Address
```

The client does NOT supply:

- UserId;
- PNR;
- Status;
- TotalFare;
- seat allocation;
- email.

### View Reservation

```http
GET /api/reservations/{pnr}
```

Passenger role required.

Ownership MUST be enforced.

### Cancel

```http
POST /api/reservations/{pnr}/cancel
```

Passenger role required.

### Availability

```http
GET /api/reservations/availability
```

Actual availability is computed by Reservation Service.

## 11.5 Payment APIs

Internal:

```http
POST /api/payments/process
POST /api/payments/refund
```

Both endpoints require:

```text
X-Internal-Service-Key
Idempotency-Key
```

`X-Internal-Service-Key` authenticates the calling internal service. `Idempotency-Key` prevents duplicate payment/refund effects and is persisted uniquely by Payment Service. Neither value is hardcoded in source code.

## 11.6 Dummy Razorpay APIs

```http
POST /api/dummy-razorpay/process
POST /api/dummy-razorpay/refund
```

## 11.7 Mail API

Internal:

```http
POST /api/mail/send
```

Requires `X-Internal-Service-Key` and is not routed through the API Gateway.

Templates:

```text
BookingConfirmed
BookingWaitlisted
Cancellation
WaitlistPromotion
```

## 11.8 HTTP Statuses

Use appropriately:

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
500 Internal Server Error
```

## 11.9 Error Response

```json
{
  "statusCode": 400,
  "error": "ValidationError",
  "message": "Invalid journey route.",
  "traceId": "..."
}
```

Controllers remain thin.

---

## 11.10 Representative DTOs

The implementation should use simple request/response DTOs at API boundaries. Representative shapes from the LLD are:

```csharp
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, int UserId, string Role, int ExpiresIn);
public record BookingPassengerRequest(string Name, int Age, Gender Gender, string Address);
public record BookingRequest(
    int TrainId,
    int FromStationId,
    int ToStationId,
    DateTime JourneyDate,
    CoachType CoachType,
    QuotaType Quota,
    List<BookingPassengerRequest> Passengers);
public record BookingResponse(
    string Pnr,
    BookingStatus Status,
    decimal TotalFare,
    List<BookingPassengerResponse> Passengers);
public record BookingPassengerResponse(
    int BookingPassengerId,
    string Name,
    string? CoachNumber,
    string? SeatNumber);
```

These are representative examples, not permission to redesign the API contract.

# 12. Authentication and Authorization Contract

## 12.1 JWT

User Service issues JWTs.

JWT should contain at minimum:

- UserId / NameIdentifier;
- Role.

It may also contain:

- issuer;
- audience;
- expiry;
- issued-at.

## 12.2 Registration

Registration is public.

A new registration always gets:

```text
Passenger
```

The client cannot choose Administrator.

Administrator users must be provisioned through controlled mechanisms.

## 12.3 Public Operations

No JWT required for:

- registration;
- login;
- train search;
- train details;
- schedule;
- fare;
- actual availability.

## 12.4 Passenger Operations

Passenger role required for:

- booking;
- viewing own reservation;
- cancelling own reservation.

## 12.5 Administrator Operations

Administrator role required for:

- train management;
- station management;
- route/schedule management;
- coach management;
- seat management;
- fare management.

## 12.6 Gateway

Gateway validates:

- signature;
- issuer;
- audience;
- expiry;
- role.

ASP.NET Core mechanisms should include:

```csharp
AddAuthentication()
AddJwtBearer()
AddAuthorization()
```

and appropriate:

```csharp
[Authorize]
[Authorize(Roles = "Administrator")]
```

## 12.7 Defense in Depth

Services may validate authentication/authorization themselves.

The Gateway must not be the only protection for sensitive operations.

## 12.8 HTTP Authentication Errors

```text
401 Unauthorized
```

means missing/invalid JWT.

```text
403 Forbidden
```

means authenticated identity lacks the required role or ownership permission.

---

# 13. Business Rules and Validation Contract

All business validation MUST happen server-side.

## 13.1 Passenger Count

A booking must contain:

```text
1 ≤ passenger count ≤ 6
```

## 13.2 Passenger Validation

Validate:

- required name;
- valid age;
- valid gender;
- address where required by the API contract.

## 13.3 Journey Validation

Validate:

- journey date;
- train existence;
- origin station;
- destination station;
- route order;
- origin != destination.

## 13.4 Fare

Train Service provides the current applicable fare.

The client MUST NOT be trusted to provide the final fare.

Adults and children are charged the applicable full fare.

No child discount is implemented.

The final charged amount is stored in:

```text
Booking.TotalFare
```

## 13.5 Availability

Availability is dynamic.

Do not use a static `IsAvailable` seat flag.

The overlap formula is:

```text
existingFromOrder < requestedToOrder
AND
requestedFromOrder < existingToOrder
```

## 13.6 Atomic Booking

For 1–6 passengers:

- if enough seats exist for all → Confirmed;
- otherwise → Waitlisted.

Never partially confirm.

## 13.7 Payment

Payment must succeed before final booking state is persisted.

### Payment failure

If payment fails:

```text
No confirmed booking
No waitlist entry
```

### Persistence failure after payment

If payment succeeds but reservation persistence fails:

```text
Attempt compensating refund
Do not create a completed reservation
Surface/log the failure
```

Do not introduce a full distributed Saga framework.

## 13.8 Cancellation

Cancellation requires:

- authenticated Passenger;
- valid PNR;
- booking belongs to current user;
- booking is not already cancelled;
- journey has not started.

Cancellation gives full refund.

## 13.9 Seat Release

Canceling a confirmed booking releases its allocations by making those allocations no longer participate in future availability calculations.

Do not toggle an `IsAvailable` flag.

## 13.10 Waitlist Promotion

After seat release:

1. find the earliest eligible waiting booking;
2. calculate available seats;
3. promote only if enough seats exist for all passengers;
4. allocate all required seats;
5. change status to Confirmed;
6. remove/complete its waitlist entry;
7. send promotion notification.

Do not skip the first waiting booking.

No second payment is required.

## 13.11 Journey Start

After journey start:

- no further waitlist promotion;
- automatic expiry/cancellation of old waiting bookings is outside scope.

## 13.12 Email

Email is collected at registration and stored on User.

Reservation retrieves the registered email from User Service.

Booking and BookingPassenger do not store email.

Email failure does NOT roll back a successful booking. Failed notifications are logged with the booking PNR for manual retry; automated retry queues/DLQs are out of scope.

## 13.13 Additional LLD Version 1.5 Clarifications

The following rules are explicitly frozen from the finalized LLD Version 1.5 and MUST be followed during implementation.

### Payment and Reservation Transaction Boundary

The booking workflow is:

```text
Generate booking identifier
        ↓
Process payment
        ↓
Reservation DB transaction
        ↓
Persist booking / passengers / allocations or waitlist
```

Payment and refund calls are external operations and are NOT part of the Reservation DB transaction.

If payment succeeds but the Reservation DB transaction/persistence fails, Reservation Service MUST request a compensating refund through Payment Service. No completed reservation is returned.

The Reservation DB transaction covers the reservation-side atomic work required for availability validation, seat allocation, booking persistence, passenger persistence, and waitlist persistence/promotion.

### Segment Overlap and StopOrder

`FromStationId` and `ToStationId` identify stations on the train route. Their corresponding `RouteStops.StopOrder` values are used for segment comparison.

Two seat allocations overlap when:

```text
existingFromOrder < requestedToOrder
AND
requestedFromOrder < existingToOrder
```

A physical seat may therefore be reused on non-overlapping journey segments.

### Concurrency

Availability validation and seat allocation MUST occur inside the same Reservation DB transaction. SQL Server `SERIALIZABLE` isolation or an equivalent explicit locking approach MUST prevent concurrent requests from allocating the same physical seat for overlapping segments.

Do NOT use Redis or distributed locks.

### Coach and Seat Identity

Once a coach or seat has been referenced by a reservation/seat allocation, its identifier MUST NOT be deleted or reused for a different physical coach/seat.

### Internal Service Security

Payment Service and Mail Service internal HTTP endpoints MUST require the configured `X-Internal-Service-Key`.

The key MUST come from configuration/environment and MUST NOT be hardcoded. These internal endpoints are not routed through the API Gateway.

### Payment and Refund Idempotency

Payment and refund requests MUST include an `Idempotency-Key`. The key MUST be persisted uniquely in Payment DB.

`Idempotency-Key` is separate from the provider `TransactionReference`. Repeated requests with the same idempotency key must not create duplicate payment/refund effects.

### Cancellation Cutoff

Cancellation is permitted until the scheduled departure time of the train from the booking's `FromStation`. After that scheduled departure time, cancellation is rejected.

### Quota Rules

- Ladies quota requires ALL passengers in the booking to be female.
- General quota has no gender restriction.
- General and Ladies quota bookings share the same physical seat pool.
- Quota is a booking-level attribute; separate quota-specific seat inventory is out of scope for the MVP.

### Waitlist Position Rules

Waitlist position is assigned when the booking enters the waitlist. Positions are NOT renumbered after promotion or cancellation. Gaps are allowed. FIFO evaluation uses ascending original position.

There is no explicit maximum waitlist length in the current MVP. Operational capacity limits are out of scope.

A waiting booking is eligible for promotion only until journey start. No automatic expiry/cancellation mechanism for old waiting bookings is required in the MVP.

### Notification Failure

Notification failure MUST NOT roll back a successful booking, cancellation, or waitlist promotion. Failed notifications MUST be logged with the booking PNR so that they can be manually retried. Automated retry queues and dead-letter queues are out of scope for the MVP.

### Audit Fields

`Booking.UpdatedAt` and `Booking.CancelledAt` are required audit fields. They do not require dedicated indexes for the current MVP unless a concrete implementation need is identified.

### Performance Scope

Load testing, performance benchmarking, and production throughput/capacity targets are outside the current MVP/LLD scope. Do not introduce performance infrastructure merely to satisfy these non-requirements.


---

# 14. Service Implementation Guidelines

## 14.1 General Layering

Use:

```text
Controller
→ Application Service
→ Repository Interface
→ Repository
→ DbContext
```

Use:

```text
Application Service
→ Client Interface
→ HTTP Client
→ Remote Service
```

## 14.2 User Service Implementation Order

Implement:

1. Role entity;
2. User entity;
3. DbContext;
4. migrations;
5. role seed;
6. repository;
7. password hashing;
8. registration;
9. login;
10. JWT;
11. user lookup;
12. controller;
13. tests.

## 14.3 Train Service Implementation Order

Implement:

1. entities;
2. DbContext;
3. configurations;
4. migrations;
5. seed data;
6. repositories;
7. public service;
8. admin service;
9. controllers;
10. validation;
11. tests.

## 14.4 Dummy Razorpay

Implement:

- process endpoint;
- refund endpoint;
- deterministic success/failure behavior;
- DTOs;
- tests.

No DB.

## 14.5 Payment Service

Implement:

- Payment entity;
- DbContext;
- repository;
- gateway interface;
- Dummy Razorpay HTTP client;
- processing;
- refund;
- statuses;
- tests.

## 14.6 Mail Service

Implement:

- request DTO;
- mail service;
- SMTP sender;
- templates;
- controller;
- tests.

No DB.

## 14.7 Reservation Service

Implement incrementally:

1. DB/domain;
2. external clients;
3. availability;
4. booking;
5. cancellation;
6. waitlist;
7. promotion;
8. concurrency;
9. comprehensive tests.

The booking/cancellation/promotion workflows must use the Reservation DB transaction boundaries and SQL Server concurrency protection defined in Section 13.13.

Reservation remains one deployable service.

## 14.8 API Gateway

Implement last among core runtime services.

Responsibilities remain limited to:

- routing;
- JWT validation;
- authorization.

---

# 15. Implementation Phases and Execution Plan

## Phase 0 — Solution and Infrastructure Skeleton

Create:

- solution;
- projects;
- test projects;
- global.json;
- base configuration;
- README;
- .gitignore;
- .editorconfig;
- shared/global exception middleware structure where required by each HTTP service.

Verify:

```text
dotnet --version
dotnet build
dotnet test
```

Do not proceed until the skeleton builds.

## Phase 1 — User Service

Implement:

- User;
- Role;
- User DB;
- migrations;
- seed roles;
- registration;
- login;
- password hashing;
- JWT;
- role claim;
- user lookup;
- tests.

Gate:

- build passes;
- tests pass;
- registration creates Passenger;
- login produces valid JWT.

## Phase 2 — Train Service

Implement:

- Train;
- Station;
- RouteStop;
- Coach;
- Seat;
- Fare;
- DB;
- migrations;
- deterministic seed;
- public APIs;
- admin APIs;
- tests.

Do not introduce:

- Route entity;
- TrainRun;
- TrainInstance;
- IsAvailable.

## Phase 3 — Dummy Razorpay Gateway

Implement:

- payment endpoint;
- refund endpoint;
- deterministic simulation;
- failure behavior;
- tests.

No DB.

## Phase 4 — Payment Service

Implement:

- Payment entity;
- Payment DB;
- repository;
- payment gateway abstraction;
- Dummy Razorpay HTTP integration;
- process;
- refund;
- tests.

## Phase 5 — Mail Service

Implement:

- Mail Service;
- SMTP sender;
- templates;
- endpoint;
- tests.

No DB.

## Phase 6 — Reservation Service

### 6A — Database and Domain

Implement:

- Booking;
- BookingPassenger;
- SeatAllocation;
- WaitlistEntry;
- enums;
- DbContext;
- repositories;
- migrations.

### 6B — External Clients

Implement:

- IUserClient;
- ITrainClient;
- IPaymentClient;
- IMailClient.

### 6C — Availability

Implement segment-based availability.

### 6D — Booking

Implement:

- validation;
- fare retrieval;
- passenger validation;
- availability;
- payment;
- atomic state;
- seat allocations;
- PNR;
- registered email lookup;
- notification.

Booking workflow MUST follow: generate booking identifier → process payment → Reservation DB transaction → persist final confirmed/waitlisted state. If persistence fails after successful payment, request compensating refund.

### 6E — Cancellation

Implement:

- ownership;
- status;
- journey-start validation using the scheduled departure time from the booking's FromStation;
- refund;
- release;
- notification.

### 6F — Waitlist

Implement:

- FIFO;
- full-booking promotion;
- no second payment;
- journey-start boundary;
- promotion notification.

### 6G — Concurrency

Verify:

- simultaneous booking;
- simultaneous cancellation/promotion;
- SQL Server transaction behavior;
- suitable locking/isolation.

No Redis/distributed locks.

## Phase 7 — API Gateway

Implement:

- routing;
- JWT validation;
- RBAC;
- routes to User/Train/Reservation.

Do not route directly to Payment/Mail/Dummy.

## Phase 8 — Integration and End-to-End Testing

Test:

- public train information;
- registration;
- login;
- confirmed booking;
- waitlisted booking;
- cancellation;
- refund;
- FIFO promotion;
- payment failure;
- persistence failure;
- email failure;
- authentication failures;
- authorization failures;
- concurrency.

## Phase 9 — Final Cleanup and Documentation

Verify:

- architecture;
- code quality;
- tests;
- configuration;
- migrations;
- README;
- setup;
- API documentation;
- no unauthorized additions.

---

# 16. Testing Strategy and Quality Gates

## 16.1 Testing Levels

Use:

1. Unit tests
2. Service/integration tests
3. API tests
4. End-to-end tests

## 16.2 Unit Testing

Use NUnit.

Mock external dependencies such as:

```text
IUserClient
ITrainClient
IPaymentClient
IMailClient
IPaymentGateway
```

Test business behavior rather than implementation details.

## 16.3 Reservation Tests

Reservation tests MUST cover:

- 1 passenger;
- 6 passengers;
- more than 6;
- invalid route;
- invalid train;
- invalid station;
- unavailable seats;
- confirmed booking;
- waitlisted booking;
- payment failure;
- persistence failure;
- cancellation;
- wrong owner;
- already cancelled;
- journey started;
- FIFO;
- no waitlist skipping;
- whole-booking promotion;
- no second payment;
- no promotion after journey start;
- email failure without booking rollback;
- segment overlap.

## 16.4 Authentication Tests

Test:

- registration;
- duplicate email;
- duplicate phone;
- login;
- invalid password;
- invalid token;
- expired token;
- Passenger access;
- Administrator access;
- wrong role;
- ownership.

## 16.5 Integration Tests

Verify:

- migrations;
- schema;
- constraints;
- repositories;
- transactions;
- service-to-service HTTP.

## 16.6 Concurrency

Test concurrent attempts to reserve the same final available seats.

The implementation must prevent double allocation.

Use SQL Server transaction and locking/isolation mechanisms.

Do not use Redis or distributed locks.

## 16.7 Quality Gates

A phase is complete only when:

- code builds;
- relevant tests pass;
- migrations are valid;
- API contracts are respected;
- business rules are enforced;
- no known critical errors remain.

---

# 17. Configuration, Secrets and Environment Setup

Configuration MUST be externalized.

Do not hardcode:

- passwords;
- JWT secrets;
- DB credentials;
- SMTP credentials;
- environment-specific URLs.

## 17.1 Example Local Ports

These are example development ports:

```text
API Gateway        5000
User Service       5001
Train Service      5002
Reservation        5003
Payment Service    5004
Mail Service       5005
Dummy Razorpay     5006
```

The final local configuration must use one consistent set.

## 17.2 Configuration Areas

Each service should configure only what it owns/needs.

Examples:

```text
ConnectionStrings
Jwt
ServiceUrls
Smtp
DummyRazorpay
```

## 17.3 Inter-Service URLs

Use configuration-driven named/typed HttpClient configuration.

Reservation requires:

```text
UserServiceUrl
TrainServiceUrl
PaymentServiceUrl
MailServiceUrl
```

Payment requires:

```text
DummyRazorpayUrl
```

Gateway requires:

```text
UserServiceUrl
TrainServiceUrl
ReservationServiceUrl
```

## 17.4 Secrets

Use environment variables, user secrets, or another appropriate local secret mechanism.

Never commit secrets.

## 17.5 Startup Documentation

README/documentation must explain:

- .NET SDK;
- SQL Server;
- database setup;
- migrations;
- configuration;
- secrets;
- service startup;
- Gateway;
- Dummy Razorpay;
- SMTP.

If a command requires Windows Administrator privileges, briefly explain why immediately before the command and prefer a non-elevated alternative when possible.

---

# 18. Database Migration, Seed Data and Local Setup

## 18.1 Database Ownership

```text
User Service        → User DB
Train Service       → Train DB
Reservation Service → Reservation DB
Payment Service     → Payment DB
```

## 18.2 Migrations

Each DB-owning service has its own migrations.

Do not create a shared migration project that violates ownership.

## 18.3 Role Seed

Seed:

```text
Passenger
Administrator
```

Registration always assigns Passenger.

## 18.4 Train Seed

Use deterministic development data.

Example route:

```text
A → B → C → D → E
```

Seed:

- trains;
- stations;
- ordered route stops;
- coaches;
- seats;
- fares.

Do NOT seed:

- Route entity;
- TrainRun;
- TrainInstance;
- RAC;
- berth positions;
- IsAvailable.

## 18.5 Reservation Seed

Create schema but do not seed arbitrary real-looking bookings unless specifically needed for tests.

## 18.6 Payment Seed

Do not seed fake successful payments.

## 18.7 Mail/Dummy

No database.

## 18.8 Reset

Do not automatically drop databases.

Provide explicit documented reset procedures for development/testing.

Seed operations should be idempotent where practical.

## 18.9 Verification

After migrations:

1. inspect database in SSMS 22;
2. verify tables;
3. verify keys;
4. verify indexes;
5. verify constraints;
6. run tests.

---

# 19. Error Handling, Logging and Observability

## 19.1 Global Error Handling

Use global exception middleware. Register it early in the HTTP middleware pipeline so that it wraps downstream middleware, including authentication/authorization and endpoint execution. This ensures exceptions are consistently mapped and logged.

Conceptual:

```text
GlobalExceptionMiddleware
- InvokeAsync()
- catch exceptions
- map to HTTP status
- log
- return ErrorResponse
```

## 19.2 ErrorResponse

```text
StatusCode
Error
Message
TraceId
```

## 19.3 Exception Categories

Use application/domain exceptions for:

- validation;
- not found;
- conflict;
- unauthorized;
- forbidden.

Controllers should not contain repeated business try/catch blocks.

## 19.4 HTTP Mapping

```text
Validation      → 400
Unauthorized    → 401
Forbidden       → 403
NotFound        → 404
Conflict        → 409
Unexpected      → 500
```

## 19.5 External HTTP Failures

Inter-service failures MUST be explicit. Internal Payment and Mail endpoints MUST also validate `X-Internal-Service-Key`. Payment/refund calls must carry an `Idempotency-Key`.

Do not fabricate successful responses when a dependency is unavailable.

Use appropriate HttpClient timeout configuration.

Do not introduce unsafe automatic retries for:

- payment;
- refund;
- booking;
- cancellation.

These operations can have side effects.

## 19.6 Payment Failure Rules

```text
Payment fails
    ↓
No final booking/waitlist state
```

If payment succeeds and reservation persistence fails:

```text
Reservation persistence fails
    ↓
Attempt compensating refund
```

If refund fails, the failure must not be silently swallowed.

## 19.7 Mail Failure

If booking succeeds but email fails:

```text
Booking remains successful
Email failure logged
No booking rollback
```

## 19.8 Logging

Use:

```csharp
ILogger<T>
```

Prefer structured logging.

Do not log:

- passwords;
- JWT secrets;
- raw card data;
- CVV;
- bank credentials.

Avoid unnecessary PII.

PNR and internal IDs may be logged when useful for troubleshooting.

## 19.9 Trace IDs

Use ASP.NET Core request tracing/trace identifiers and return the trace ID in error responses.

---

# 20. Integration, Service Startup and End-to-End Execution

## 20.1 Local Topology

```text
Gateway        :5000
User Service   :5001
Train Service  :5002
Reservation    :5003
Payment        :5004
Mail           :5005
Dummy Razorpay :5006
```

Plus:

```text
SQL Server
 ├── User DB
 ├── Train DB
 ├── Reservation DB
 └── Payment DB
```

## 20.2 Startup Order

Recommended:

1. SQL Server
2. Apply migrations/seed
3. User Service
4. Train Service
5. Dummy Razorpay
6. Payment Service
7. Mail Service
8. Reservation Service
9. API Gateway
10. Client/API test tool

A process being started does not necessarily mean it is ready.

## 20.3 Public Information Flow

Example:

```text
Client
 ↓
Gateway
 ↓
Train Service
 ↓
Train DB
```

For actual accommodation availability:

```text
Client
 ↓
Gateway
 ↓
Reservation Service
 ↓
Train Service + Reservation DB
```

No login is required for public train information or availability.

## 20.4 Confirmed Booking E2E

Verify:

1. user has valid JWT;
2. passenger count is 1–6;
3. route is valid;
4. fare comes from Train Service;
5. full fare is calculated;
6. segment availability is checked;
7. Payment Service processes payment;
8. Dummy Razorpay processes payment;
9. reservation is persisted as Confirmed;
10. seats are allocated to every BookingPassenger;
11. PNR is generated;
12. historical TotalFare is stored;
13. registered email is retrieved;
14. confirmation notification is sent.

## 20.5 Waitlisted Booking E2E

Verify:

1. enough seats do not exist for the complete booking;
2. payment succeeds;
3. booking becomes Waitlisted;
4. no seat allocations are created;
5. WaitlistEntry is created;
6. PNR is generated;
7. waitlist notification is sent.

## 20.6 Cancellation E2E

Verify:

1. JWT is valid;
2. PNR exists;
3. booking belongs to caller;
4. booking is active;
5. journey has not started;
6. status becomes Cancelled;
7. allocations are released;
8. refund is processed;
9. cancellation notification is sent;
10. FIFO promotion is evaluated.

## 20.7 Promotion E2E

Verify:

- earliest eligible booking is considered first;
- no skipping;
- enough seats exist for the whole booking;
- all passengers receive allocations;
- status changes to Confirmed;
- waitlist entry is removed/completed;
- no second payment occurs;
- promotion notification is sent;
- no promotion happens after journey start.

## 20.8 Negative Tests

Test:

- payment failure;
- payment timeout;
- repeated idempotent payment request;
- repeated idempotent refund request;
- reservation persistence failure;
- compensating refund after payment success;
- duplicate PNR handling where applicable;
- email/SMTP server unavailable;
- notification failure without booking rollback;
- missing JWT;
- invalid JWT;
- expired JWT;
- wrong role;
- unauthorized cancellation by another user;
- wrong owner;
- invalid route;
- more than 6 passengers;
- invalid Ladies quota passenger composition;
- concurrent booking of the final available seat;
- insufficient seats for full waitlist promotion;
- attempted promotion after journey start;
- cancellation after the FromStation departure cutoff;
- duplicate registration.

## 20.9 API Verification

Use:

- Swagger;
- Postman;
- curl;
- automated API tests.

Normal end-to-end flow should pass through the Gateway.

## 20.10 Deterministic Test Data

Keep local test data deterministic.

Document how to reset/reseed development databases.

---

# 21. Coding Standards and Definition of Done

## 21.1 General C#

Use modern, readable C#.

Prefer:

- focused classes;
- focused methods;
- meaningful names;
- nullable reference types;
- async I/O;
- constructor injection;
- simple abstractions.

Avoid unnecessary abstraction.

## 21.2 Naming

Use:

```text
PascalCase
```

for:

- classes;
- interfaces;
- methods;
- properties.

Use:

```text
camelCase
```

for:

- parameters;
- local variables.

Use:

```text
_camelCase
```

for private fields.

## 21.3 Async

Use `async`/`await` for DB and HTTP I/O.

Avoid:

```csharp
.Result
.Wait()
```

Do not introduce cancellation tokens everywhere unless they provide practical value.

## 21.4 Dependency Injection

Use built-in ASP.NET Core DI.

Prefer constructor injection.

Do not instantiate dependencies directly with `new` inside application logic.

## 21.5 Controllers

Controllers should:

- receive requests;
- bind DTOs;
- access authenticated identity;
- call application services;
- return HTTP responses.

Controllers should NOT contain business workflows or direct repository access.

## 21.6 Application Services

Application services contain:

- business workflow;
- validation orchestration;
- transaction boundaries;
- external service coordination.

## 21.7 Repositories

Repositories contain data access only.

They must not:

- call HTTP services;
- return HTTP responses;
- contain complete business workflows;
- access another service's database.

## 21.8 DTOs

Keep API DTOs separate from EF entities.

Do not expose:

- password hash;
- payment secrets;
- internal implementation details.

## 21.9 Comments

Comments should explain WHY when useful.

Do not add comments that merely restate obvious code.

## 21.10 Definition of Done

A feature is complete only when:

- requirements are implemented;
- architecture is preserved;
- DB changes are migrated;
- APIs work;
- authentication/authorization is correct;
- business rules are enforced;
- tests exist;
- tests pass;
- configuration is externalized;
- errors are handled;
- logs are appropriate;
- no placeholders remain;
- documentation is updated where needed.

---

# 22. AI Agent Execution Instructions

## 22.1 Agent Role

Act as an implementation engineer working under an approved architecture.

Do not act as an independent architect.

## 22.2 Required Inputs

Before implementation, the agent should have access to:

- finalized LLD;
- this guide;
- repository;
- .NET 10 SDK;
- SQL Server 2019.

## 22.3 Initial Repository Inspection

Before modifying anything, inspect:

- solution;
- projects;
- existing source;
- tests;
- configuration;
- migrations;
- README;
- package references.

Do not assume the repository is empty.

## 22.4 Execution Loop

Use:

```text
Understand
   ↓
Inspect
   ↓
Plan small change
   ↓
Implement
   ↓
Build
   ↓
Test
   ↓
Fix
   ↓
Review against LLD
   ↓
Report
   ↓
Continue
```

## 22.5 Phase Execution

Execute phases in order:

```text
Phase 0
   ↓
Phase 1
   ↓
Phase 2
   ↓
Phase 3
   ↓
Phase 4
   ↓
Phase 5
   ↓
Phase 6
   ↓
Phase 7
   ↓
Phase 8
   ↓
Phase 9
```

Do not move to the next phase while the current phase has unresolved critical failures.

## 22.6 Explicitly Forbidden Additions

Do not introduce:

- Kafka;
- RabbitMQ;
- Redis;
- Kubernetes;
- service discovery;
- CQRS;
- event sourcing;
- distributed locks;
- full Saga frameworks;
- additional microservices;
- TrainRun;
- TrainInstance;
- Route;
- RAC;
- berth-position modeling;
- Seat.IsAvailable;
- shared database;
- cross-service EF references;
- direct cross-service database access.

## 22.7 Database Change Workflow

Whenever schema changes:

```text
Entity
 ↓
EF configuration
 ↓
Migration
 ↓
Review migration
 ↓
Apply migration
 ↓
Inspect with SSMS
 ↓
Test
```

## 22.8 Security

Never:

- bypass authentication;
- accept client-supplied role during registration;
- trust client-supplied final fare;
- store raw card credentials;
- expose password hashes;
- log secrets.

## 22.9 Ambiguity

If a decision is clearly implied by the LLD, choose the simplest compatible implementation.

If it materially changes:

- architecture;
- domain;
- API;
- DB;
- security;
- business behavior;

stop and ask.

## 22.10 Recommended Master Prompt

The following prompt may be provided to an AI coding agent together with this guide:

```text
You are the implementation engineer for the Railway Reservation System.

Treat the finalized Railway Reservation System LLD, Version 1.5, as the
primary design authority and AI_IMPLEMENTATION_GUIDE.md as the execution
contract.

Before changing code, inspect the repository and understand the existing
implementation.

Implement incrementally, phase by phase. After each meaningful change,
build the solution, run relevant tests, fix failures, and verify the
implementation against the LLD.

Preserve the approved architecture:

- API Gateway
- User Service
- Train Service
- Reservation Service
- Payment Service
- Mail Service
- Dummy Razorpay Gateway as the external payment simulator

Preserve database ownership:

- User DB
- Train DB
- Reservation DB
- Payment DB

Never introduce shared databases or cross-service EF references.

Reservation remains one microservice containing Booking, Availability,
and Waitlist components.

Preserve JWT authentication, Passenger/Administrator RBAC, ownership
checks, segment-based availability, atomic 1–6 passenger booking,
confirmed/waitlisted outcomes, payment-before-final-state behavior,
compensating refund after persistence failure, cancellation/refund,
strict FIFO waitlist promotion, full-booking promotion, no second
payment after promotion, and the journey-start waitlist boundary.

Do not introduce Kafka, RabbitMQ, Redis, Kubernetes, service discovery,
CQRS, event sourcing, distributed locks, full Saga frameworks, TrainRun,
TrainInstance, Route, RAC, berth-position modeling, Seat.IsAvailable,
or additional microservices unless explicitly approved.

Do not silently change APIs, entities, database ownership, security,
business rules, or technology choices.

Use C#/.NET 10, ASP.NET Core, EF Core, SQL Server 2019, JWT Bearer,
HTTP/REST, SMTP, and NUnit.

Keep controllers thin, use application services for workflows,
repositories for data access, and client abstractions for inter-service
HTTP communication.

Code must be easy to understand for a developer familiar with basic
C#/.NET. Prefer explicit, straightforward implementations over clever
or highly abstract code. Use simple control flow, descriptive names,
small methods, explicit DTO mapping, clear EF Core queries, and basic
LINQ where appropriate. Avoid advanced techniques and unnecessary
abstractions when a simpler implementation satisfies the requirement.
Do not optimize for brevity; optimize for readability and
understandability.

Never store raw card numbers, CVV, bank credentials, or password
plaintext.

If you encounter an ambiguity that materially affects architecture,
domain, API, database, security, or business behavior, stop and ask
before making an unauthorized design decision.

The goal is the smallest correct, maintainable, testable implementation
that matches the approved LLD.
```

## 22.11 Completion Reporting

At the end of each phase report:

```text
Phase:
Implemented:
Files/projects changed:
Database changes:
APIs added/changed:
Tests added:
Build result:
Test result:
Known issues:
LLD compliance:
Next phase:
```

---

# 23. Final Implementation Checklist

This checklist is the final verification gate.

Do not declare the project complete until implementation and verification support the relevant items.

## 23.1 Solution

- [ ] Solution builds.
- [ ] .NET 10 is used.
- [ ] global.json is present/configured.
- [ ] Test projects exist.
- [ ] README exists.
- [ ] .gitignore exists.
- [ ] .editorconfig exists.

## 23.2 Services

- [ ] API Gateway exists.
- [ ] User Service exists.
- [ ] Train Service exists.
- [ ] Reservation Service exists.
- [ ] Payment Service exists.
- [ ] Mail Service exists.
- [ ] Dummy Razorpay Gateway exists.
- [ ] No unauthorized microservices were added.

## 23.3 Database Architecture

- [ ] User DB exists.
- [ ] Train DB exists.
- [ ] Reservation DB exists.
- [ ] Payment DB exists.
- [ ] Each DB has one owning DbContext.
- [ ] Migrations exist.
- [ ] Schema was verified.
- [ ] No shared DB exists.
- [ ] No cross-service FK exists.
- [ ] No cross-service EF navigation exists.
- [ ] No harmful cascading design exists.

## 23.4 User Service

- [ ] User entity matches contract.
- [ ] Role entity matches contract.
- [ ] Passenger role seeded.
- [ ] Administrator role seeded.
- [ ] Registration validates input.
- [ ] Registration enforces uniqueness.
- [ ] Registration assigns Passenger.
- [ ] Client cannot select Administrator.
- [ ] Passwords are hashed.
- [ ] Login works.
- [ ] JWT is generated.
- [ ] JWT contains identity and role.
- [ ] User lookup does not expose password hash.

## 23.5 Train Service

- [ ] Train implemented.
- [ ] Station implemented.
- [ ] RouteStop implemented.
- [ ] Coach implemented.
- [ ] Seat implemented.
- [ ] Fare implemented.
- [ ] Route ordering is validated.
- [ ] Public train APIs work.
- [ ] Admin APIs work.
- [ ] Current fare comes from Train Service.
- [ ] No Route entity.
- [ ] No TrainRun.
- [ ] No TrainInstance.
- [ ] No Seat.IsAvailable.

## 23.6 Dummy Razorpay

- [ ] Separate application exists.
- [ ] Process endpoint works.
- [ ] Refund endpoint works.
- [ ] Deterministic success/failure behavior exists.
- [ ] No database exists.
- [ ] Only Payment Service calls it.

## 23.7 Payment Service

- [ ] Payment entity implemented.
- [ ] Payment DB implemented.
- [ ] Payment repository implemented.
- [ ] Payment service implemented.
- [ ] Gateway abstraction implemented.
- [ ] Dummy Razorpay communication works.
- [ ] Successful payment works.
- [ ] Failed payment works.
- [ ] Refund works.
- [ ] Payment status is persisted.
- [ ] No raw card/CVV/bank credentials are stored.

## 23.8 Mail Service

- [ ] Mail service implemented.
- [ ] SMTP sender implemented.
- [ ] Required templates exist.
- [ ] Booking confirmation works.
- [ ] Waitlist notification works.
- [ ] Cancellation notification works.
- [ ] Promotion notification works.
- [ ] No database exists.
- [ ] Mail failure does not roll back booking.

## 23.9 Reservation Service

- [ ] Booking has `UpdatedAt` and `CancelledAt`.
- [ ] Ladies quota requires all passengers to be female.
- [ ] General/Ladies bookings share the same physical seat pool.
- [ ] Seat availability uses RouteStop `StopOrder` segment overlap.
- [ ] Availability validation and allocation are protected by the required Reservation DB transaction/concurrency approach.

- [ ] Booking implemented.
- [ ] BookingPassenger implemented.
- [ ] SeatAllocation implemented.
- [ ] WaitlistEntry implemented.
- [ ] 1–6 passengers enforced.
- [ ] Passenger validation implemented.
- [ ] Route validation implemented.
- [ ] Fare retrieved from Train Service.
- [ ] Full fare rule implemented.
- [ ] Historical TotalFare stored.
- [ ] Dynamic segment availability implemented.
- [ ] Atomic confirmed/waitlisted booking implemented.
- [ ] Payment required before final state.
- [ ] Payment failure leaves no final booking state.
- [ ] Persistence failure after payment triggers compensating refund.
- [ ] PNR generated uniquely.
- [ ] BookingPassengerId maps passenger to seat.
- [ ] Registered email retrieved from User Service.
- [ ] Booking response is correct.

## 23.10 Cancellation

- [ ] Cancellation cutoff uses scheduled departure from the booking's FromStation.
- [ ] Cancellation is rejected after that cutoff.

- [ ] User can cancel only own booking.
- [ ] Already cancelled booking cannot be cancelled again.
- [ ] Cancellation before journey start enforced.
- [ ] Booking becomes Cancelled.
- [ ] Seats are released logically through allocation state.
- [ ] Refund is processed.
- [ ] Cancellation notification is sent.
- [ ] Waitlist evaluation follows cancellation.

## 23.11 Waitlist

- [ ] Waitlist positions are not renumbered; gaps are allowed.
- [ ] FIFO uses original ascending position.
- [ ] No explicit maximum waitlist length is imposed in the MVP.
- [ ] No promotion occurs after journey start.

- [ ] FIFO ordering implemented.
- [ ] No skipping.
- [ ] Entire waiting booking must fit.
- [ ] All passengers promoted together.
- [ ] Promotion allocates seats.
- [ ] Waitlist entry is removed/completed.
- [ ] Status becomes Confirmed.
- [ ] No second payment.
- [ ] Promotion notification sent.
- [ ] No promotion after journey start.

## 23.12 Authentication and RBAC

- [ ] Public endpoints work without JWT where intended.
- [ ] Passenger endpoints require authentication.
- [ ] Admin endpoints require Administrator role.
- [ ] Gateway validates JWT.
- [ ] Services provide defense-in-depth where appropriate.
- [ ] Missing/invalid JWT gives 401.
- [ ] Wrong role gives 403.
- [ ] Ownership is enforced.
- [ ] Administrator cannot be selected through normal registration.

## 23.13 API Gateway

- [ ] Gateway routes User.
- [ ] Gateway routes Train.
- [ ] Gateway routes Reservation.
- [ ] Gateway validates JWT.
- [ ] Gateway enforces authorization.
- [ ] Gateway has no business logic.
- [ ] Gateway has no DB.
- [ ] Gateway does not directly route Payment.
- [ ] Gateway does not directly route Mail.
- [ ] Gateway does not directly route Dummy Razorpay.

## 23.14 Errors and Logging

- [ ] Global exception middleware exists and is registered early enough to wrap downstream middleware, including authentication/authorization and endpoints.
- [ ] ErrorResponse is consistent.
- [ ] Correct HTTP status mappings are used.
- [ ] External service failures are explicit.
- [ ] No unsafe payment retries exist.
- [ ] Structured logging is used.
- [ ] Secrets are not logged.
- [ ] Trace IDs are available.

## 23.15 Testing

- [ ] Negative/failure scenarios from Section 20.8 are covered.
- [ ] Final-seat concurrency is tested.
- [ ] Payment/refund idempotency is tested.
- [ ] Persistence-failure compensating refund is tested.
- [ ] SMTP failure is tested without booking rollback.

- [ ] Unit tests pass.
- [ ] Integration tests pass.
- [ ] API tests pass.
- [ ] End-to-end tests pass.
- [ ] Booking rules tested.
- [ ] Cancellation rules tested.
- [ ] Waitlist rules tested.
- [ ] Payment failure tested.
- [ ] Persistence failure tested.
- [ ] Refund tested.
- [ ] Email failure tested.
- [ ] Authentication tested.
- [ ] Authorization tested.
- [ ] Ownership tested.
- [ ] Concurrency tested.

## 23.16 End-to-End Verification

Verify the complete flows:

```text
Register
   ↓
Login
   ↓
Search/View Train
   ↓
Check Availability
   ↓
Book
   ↓
Payment
   ↓
Confirmed / Waitlisted
   ↓
View Reservation
   ↓
Cancel
   ↓
Refund
   ↓
Seat Release
   ↓
FIFO Waitlist Evaluation
   ↓
Promotion if eligible
```

## 23.17 Configuration and Security

- [ ] `X-Internal-Service-Key` is configured externally and required for internal Payment/Mail endpoints.
- [ ] Payment and refund use persisted unique `Idempotency-Key` values.
- [ ] No secrets are hardcoded.

- [ ] No secrets are committed.
- [ ] DB connections are configuration-driven.
- [ ] JWT configuration is externalized.
- [ ] Service URLs are configurable.
- [ ] SMTP configuration is externalized.
- [ ] Dummy Razorpay configuration is externalized.
- [ ] Environment-specific values are not hardcoded.
- [ ] Local setup is documented.

## 23.18 Documentation

- [ ] README updated.
- [ ] Startup order documented.
- [ ] Database setup documented.
- [ ] Migration instructions documented.
- [ ] Seed data documented.
- [ ] Service URLs documented.
- [ ] API usage documented.
- [ ] Test instructions documented.
- [ ] Known limitations documented.

## 23.19 Final Build/Test Gate

Run:

```bash
dotnet build
dotnet test
```

Both must succeed before final completion.

## 23.20 Final Architecture Compliance Gate

The implementation MUST also be checked against every frozen LLD Version 1.5 clarification in Section 13.13, including transaction boundaries, concurrency, internal API security, idempotency, cancellation cutoff, quota rules, waitlist positions, notification behavior, audit fields, and MVP performance scope.


Before declaring completion, verify that the implementation did NOT introduce:

- [ ] shared database;
- [ ] cross-service EF references;
- [ ] cross-service DB access;
- [ ] unauthorized microservice;
- [ ] Kafka;
- [ ] RabbitMQ;
- [ ] Redis;
- [ ] Kubernetes;
- [ ] service discovery;
- [ ] CQRS;
- [ ] event sourcing;
- [ ] distributed locks;
- [ ] TrainRun;
- [ ] TrainInstance;
- [ ] Route entity;
- [ ] RAC;
- [ ] berth-position modeling;
- [ ] Seat.IsAvailable.

## 23.21 Final AI Completion Declaration

The implementation agent may declare completion only after verification:

```text
Implementation Status: COMPLETE

LLD Compliance: VERIFIED
Architecture Compliance: VERIFIED
Database Ownership: VERIFIED
Authentication/RBAC: VERIFIED
Booking Rules: VERIFIED
Cancellation/Refund: VERIFIED
Seat Allocation: VERIFIED
FIFO Waitlist: VERIFIED
Concurrency: VERIFIED
Error Handling: VERIFIED
Tests: PASSING
Build: PASSING
End-to-End Verification: PASSING

No unauthorized architectural components were introduced.
No unresolved critical implementation issues remain.
```

## Final Principle

The project is complete only when:

1. the implementation matches the approved LLD;
2. the six core services and Dummy Razorpay Gateway work;
3. database ownership is preserved;
4. authentication and authorization are enforced;
5. booking, payment, cancellation, seat allocation, and waitlist rules are enforced;
6. payment and persistence failure scenarios are handled;
7. concurrency is handled safely;
8. automated tests pass;
9. end-to-end flows work;
10. the implementation remains simple and maintainable.

**Core principle: implement the smallest correct solution that faithfully realizes the approved design.**
