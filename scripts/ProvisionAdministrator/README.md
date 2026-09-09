# Provision Administrator

This is a local developer and presentation utility. It provisions one Administrator user in the existing User Service database. It is not a microservice and does not change the public registration API.

## Prerequisites

- .NET 10 SDK
- RailwayUserDb with the User Service migration applied
- The seeded `Administrator` role

## Local configuration

Create `scripts/ProvisionAdministrator/appsettings.Development.local.json` from `appsettings.Development.local.json.example` and supply the local `ConnectionStrings:UserDb` value. The local file is ignored by Git.

For convenience on an existing development machine, the utility also uses the ignored `src/UserService/appsettings.Development.local.json` when no utility-local configuration file exists.

## Run

From the repository root:

```powershell
dotnet run --project scripts/ProvisionAdministrator
```

The utility prompts for:

```text
Administrator name: Demo Administrator
Email: admin.demo@example.com
Phone number: 9999999999
Password: [entered interactively]
```

The password is not echoed, logged, stored in configuration, or printed. The utility hashes it with the same `PasswordHasher<User>` used by User Service, retrieves the existing `Administrator` role, and saves the resulting user.

If the email or phone number already belongs to any user, the utility makes no changes. It never changes an existing Passenger into an Administrator and never resets an existing password.

## Using the account

Log in through User Service:

```text
POST /api/auth/login
```

The response has `role: Administrator` and a JWT. In Train Service Swagger, select **Authorize**, enter `Bearer <token>`, and then call an Administrator endpoint.

Public registration remains unchanged: it always creates Passenger users and never accepts a role from the caller.
