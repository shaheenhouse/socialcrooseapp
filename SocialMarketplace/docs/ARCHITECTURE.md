# Architecture Documentation

## Overview

The Social Marketplace Platform follows a modern, scalable architecture designed for high performance and maintainability.

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Load Balancer                             │
└─────────────────────────────────────────────────────────────────┘
                               │
         ┌─────────────────────┼─────────────────────┐
         │                     │                     │
         ▼                     ▼                     ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│    API Pod 1    │  │    API Pod 2    │  │    API Pod N    │
│  (ASP.NET Core) │  │  (ASP.NET Core) │  │  (ASP.NET Core) │
└─────────────────┘  └─────────────────┘  └─────────────────┘
         │                     │                     │
         └─────────────────────┼─────────────────────┘
                               │
         ┌─────────────────────┼─────────────────────┐
         │                     │                     │
         ▼                     ▼                     ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│   PostgreSQL    │  │     Redis       │  │   SignalR Hub   │
│   (Primary)     │  │   (Cache/PubSub)│  │   (Backplane)   │
└─────────────────┘  └─────────────────┘  └─────────────────┘
         │
         ▼
┌─────────────────┐
│   PostgreSQL    │
│   (Replica)     │
└─────────────────┘
```

## Backend Architecture

### Vertical Slice Architecture (Helix)

The backend is organized by feature slices rather than traditional layers:

```
Marketplace.Slices/
├── Users/
│   ├── UserRepository.cs      # Data access
│   ├── UserService.cs         # Business logic
│   └── Contracts/             # DTOs
│
├── Social/
│   ├── Connections/
│   │   ├── ConnectionRepository.cs
│   │   └── ConnectionService.cs
│   ├── Follows/
│   │   ├── FollowRepository.cs
│   │   └── FollowService.cs
│   ├── Messaging/
│   │   ├── MessageRepository.cs
│   │   ├── ConversationRepository.cs
│   │   └── MessageService.cs
│   └── Search/
│       ├── SearchRepository.cs
│       └── SearchService.cs
│
├── Marketplace/
│   ├── Services/
│   ├── Products/
│   └── Orders/
│
└── Projects/
    ├── Projects/
    ├── Bids/
    └── Tenders/
```

### Benefits
- **Cohesion**: Related code stays together
- **Maintainability**: Easy to understand and modify features
- **Scalability**: Slices can be extracted to microservices
- **Testing**: Features can be tested in isolation

## Core Infrastructure

### Connection Factory

Manages database connections with read/write splitting:

```csharp
public interface IConnectionFactory
{
    IDbConnection CreateReadConnection();
    IDbConnection CreateWriteConnection();
}
```

### Multilevel Cache

Two-tier caching strategy:

```
┌─────────────────┐
│   Application   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐     Miss     ┌─────────────────┐
│  L1 Memory      │ ───────────► │   L2 Redis      │
│  (In-Process)   │              │   (Distributed) │
└─────────────────┘              └─────────────────┘
         │                                │
         │ Hit                            │ Miss
         ▼                                ▼
    Return Data               ┌─────────────────┐
                              │    Database     │
                              └─────────────────┘
```

### Job Queue (Redis Streams)

Reliable asynchronous processing:

```
Producer                 Redis Streams              Consumer
   │                          │                        │
   │  ──── XADD ────►        │                        │
   │                          │  ◄──── XREADGROUP ────│
   │                          │                        │
   │                          │  ◄──── XACK ──────────│
```

### Quantum Router

Probabilistic request routing for A/B testing and canary deployments:

```csharp
public class QuantumRouter
{
    public RouteDecision Route(string userId, string feature)
    {
        var hash = ComputeConsistentHash(userId, feature);
        return DetermineRoute(hash, _configuration);
    }
}
```

## Real-time Architecture

### SignalR with Redis Backplane

```
┌──────────┐     ┌──────────┐     ┌──────────┐
│ Client 1 │     │ Client 2 │     │ Client 3 │
└────┬─────┘     └────┬─────┘     └────┬─────┘
     │                │                │
     ▼                ▼                ▼
┌─────────────────────────────────────────────┐
│              SignalR Hub                     │
└─────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────┐
│              Redis Backplane                 │
│           (Pub/Sub + Streams)               │
└─────────────────────────────────────────────┘
                     │
     ┌───────────────┼───────────────┐
     ▼               ▼               ▼
┌──────────┐   ┌──────────┐   ┌──────────┐
│  Pod 1   │   │  Pod 2   │   │  Pod N   │
└──────────┘   └──────────┘   └──────────┘
```

### Hub Structure

```
Marketplace.Realtime/
├── Hubs/
│   ├── ChatHub.cs         # Messaging
│   ├── NotificationHub.cs # Push notifications
│   └── PresenceHub.cs     # Online status
└── ServiceCollectionExtensions.cs
```

## Background Workers

### Worker Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Worker Host                                 │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │Notification │  │   Email     │  │  Payment    │              │
│  │   Worker    │  │   Worker    │  │   Worker    │              │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘              │
│         │                │                │                      │
│         └────────────────┼────────────────┘                      │
│                          │                                       │
│                          ▼                                       │
│         ┌─────────────────────────────────┐                      │
│         │         Base Worker             │                      │
│         │   (Redis Streams Consumer)      │                      │
│         └─────────────────────────────────┘                      │
└─────────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Redis Streams                               │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │notifications│  │    email    │  │  payments   │              │
│  └─────────────┘  └─────────────┘  └─────────────┘              │
└─────────────────────────────────────────────────────────────────┘
```

### Worker Types

| Worker | Stream | Purpose |
|--------|--------|---------|
| NotificationWorker | notifications | Process notification delivery |
| EmailWorker | email | Send emails via SMTP/API |
| PaymentWorker | payment-processing | Process payments |
| SearchIndexingWorker | search-indexing | Update search indices |
| RealtimePushWorker | realtime-push | Push real-time events |

## Workflow Orchestration

### Saga Pattern for Distributed Transactions

```
OrderWorkflow:
  1. ValidateOrder
  2. ProcessPayment
  3. NotifySeller
  4. CreateDeliveryTimeline
  5. SendBuyerConfirmation

On Failure:
  - CompensateStep5
  - CompensateStep4
  - CompensateStep3
  - RefundPayment
```

### Available Workflows

- **OrderWorkflow** - Complete order lifecycle
- **ProjectWorkflow** - Project creation to completion
- **TenderWorkflow** - Government tender process
- **EscrowWorkflow** - Escrow payment management
- **NotificationWorkflow** - Multi-channel notifications

## Database Design

### Entity Relationship Overview

```
Users ─────────────── Connections ─────────────── Users
  │                                                 │
  ├── Follows ──────────────────────────────────────┤
  │                                                 │
  ├── Messages ◄───── Conversations ─────► Messages │
  │                                                 │
  ├── Services ──────── Orders ──────────── Buyers  │
  │                                                 │
  └── Projects ──────── Bids ────────────── Bidders │
```

### Key Tables

- **users** - User accounts
- **connections** - LinkedIn-style connections
- **follows** - Following relationships
- **conversations** - Chat threads
- **messages** - Chat messages
- **services** - Marketplace services
- **orders** - Purchase orders
- **projects** - Posted projects
- **bids** - Project bids
- **tenders** - Government tenders

## Frontend Architecture

### App Structure

```
src/
├── app/                    # Next.js App Router
│   ├── (public)/          # Public pages
│   │   ├── page.tsx       # Home
│   │   └── auth/          # Auth pages
│   └── dashboard/         # Protected pages
│       ├── layout.tsx     # Dashboard layout
│       ├── page.tsx       # Dashboard home
│       ├── network/       # Connections
│       ├── messages/      # Messaging
│       ├── marketplace/   # Services
│       ├── projects/      # Projects
│       └── search/        # Search
│
├── components/
│   ├── ui/                # Shadcn components
│   ├── global-search.tsx  # Command palette
│   ├── language-selector.tsx
│   ├── feature-flag.tsx
│   └── role-guard.tsx
│
├── store/                 # Zustand stores
│   ├── auth-store.ts
│   ├── language-store.ts
│   └── feature-flags-store.ts
│
└── i18n/                  # Internationalization
    ├── config.ts
    └── messages/
        ├── en.json
        ├── ur.json
        └── ...
```

### State Management

```
┌─────────────────────────────────────────────────────────────────┐
│                      Zustand Stores                              │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │  Auth Store │  │ Language    │  │  Feature    │              │
│  │             │  │   Store     │  │   Flags     │              │
│  └─────────────┘  └─────────────┘  └─────────────┘              │
├─────────────────────────────────────────────────────────────────┤
│                      React Query                                 │
│              (Server State Management)                           │
└─────────────────────────────────────────────────────────────────┘
```

## Security

### Authentication Flow

```
┌──────────┐     ┌──────────┐     ┌──────────┐
│  Client  │     │   API    │     │ Database │
└────┬─────┘     └────┬─────┘     └────┬─────┘
     │                │                │
     │  Login Request │                │
     │───────────────►│                │
     │                │  Verify User   │
     │                │───────────────►│
     │                │◄───────────────│
     │                │                │
     │                │ Generate JWT   │
     │◄───────────────│                │
     │                │                │
     │  API Request   │                │
     │  + JWT Token   │                │
     │───────────────►│                │
     │                │ Validate JWT   │
     │                │                │
     │◄───────────────│                │
```

### JWT Structure

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "roles": ["user", "seller"],
  "permissions": ["read:services", "write:services"],
  "exp": 1640000000
}
```

## Observability

### Telemetry Stack

```
┌─────────────────────────────────────────────────────────────────┐
│                      Application                                 │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │  Serilog    │  │OpenTelemetry│  │ Prometheus  │              │
│  │  (Logging)  │  │  (Tracing)  │  │  (Metrics)  │              │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘              │
└─────────┼────────────────┼────────────────┼─────────────────────┘
          │                │                │
          ▼                ▼                ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│   Console   │  │    Jaeger   │  │   Grafana   │
│   / File    │  │   / Zipkin  │  │             │
└─────────────┘  └─────────────┘  └─────────────┘
```

## Scaling Strategy

### Horizontal Scaling

- **API Pods**: Scale based on CPU/memory usage
- **Worker Pods**: Scale based on queue depth
- **SignalR Pods**: Scale based on connection count

### Database Scaling

- **Read Replicas**: Route read queries to replicas
- **Connection Pooling**: PgBouncer for connection management
- **Sharding**: Future consideration for large data

### Cache Scaling

- **Redis Cluster**: Distributed caching
- **Cache Invalidation**: Event-driven invalidation
