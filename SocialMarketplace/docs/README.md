# Social Marketplace Platform

A full-stack social marketplace application built with Next.js 15, React 19, and .NET 10, featuring LinkedIn-style networking, messaging, and a complete marketplace ecosystem.

## Overview

This platform combines the best of social networking with a robust marketplace, enabling users to:
- Connect and network with professionals
- Buy and sell services
- Post and bid on projects
- Participate in government tenders
- Manage teams and companies

## Tech Stack

### Frontend
- **Next.js 15** - React framework with App Router
- **React 19** - Latest React with concurrent features
- **Tailwind CSS 3.4** - Utility-first CSS framework
- **Shadcn UI** - Beautifully designed components
- **Framer Motion** - Production-ready animations
- **Zustand** - Lightweight state management
- **React Query** - Server state management
- **next-intl** - Internationalization (i18n)
- **next-themes** - Dark/light mode support

### Backend
- **.NET 10** - Latest .NET with performance optimizations
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 10** - ORM for database models
- **Dapper** - High-performance data access
- **PostgreSQL** - Primary database
- **Redis** - Caching, pub/sub, and job queues
- **SignalR** - Real-time communication

### Infrastructure
- **Docker** - Containerization
- **Redis Streams** - Message queuing
- **OpenTelemetry** - Distributed tracing
- **Prometheus** - Metrics collection
- **Serilog** - Structured logging

## Project Structure

```
SocialMarketplace/
├── frontend/                    # Next.js frontend
│   ├── src/
│   │   ├── app/                # App Router pages
│   │   │   ├── dashboard/      # Dashboard pages
│   │   │   └── auth/           # Authentication pages
│   │   ├── components/         # Reusable components
│   │   │   └── ui/             # Shadcn UI components
│   │   ├── store/              # Zustand stores
│   │   ├── i18n/               # Internationalization
│   │   └── lib/                # Utilities
│   └── public/                 # Static assets
│
├── backend/                     # .NET backend
│   ├── Marketplace.Api/        # Web API
│   ├── Marketplace.Database/   # EF Core models & configs
│   ├── Marketplace.Core/       # Core infrastructure
│   ├── Marketplace.Slices/     # Vertical slice features
│   ├── Marketplace.Orchestrator/ # Workflow orchestration
│   ├── Marketplace.Workers/    # Background job processors
│   └── Marketplace.Realtime/   # SignalR hubs
│
└── docs/                        # Documentation
```

## Features

### Social Features (LinkedIn-style)
- **Connections** - Send, accept, reject connection requests
- **Followers** - Follow users, companies, stores, pages
- **Messaging** - Real-time direct and group messaging
- **Search** - Unified search across all content types
- **Posts & Feed** - Share updates with reactions

### Marketplace
- **Services** - Create and sell services
- **Projects** - Post projects and receive bids
- **Orders** - Complete purchase flow with escrow
- **Reviews** - Rate and review services/sellers

### Professional Features
- **Skills & Certifications** - Verified skill tests
- **Portfolios** - Showcase work
- **Companies & Pages** - Organization profiles
- **Teams** - Manage team members and roles

### Government Tenders
- **Tender Management** - Post and browse tenders
- **Bidding** - Submit and evaluate bids
- **Contract Awards** - Award and manage contracts

### Payment & Wallet
- **Escrow** - Secure payment protection
- **Multiple Gateways** - Support for various payment providers
- **Wallet** - Balance management and payouts

## Getting Started

### Prerequisites
- Node.js 20+
- .NET 10 SDK
- PostgreSQL 16+
- Redis 7+
- Docker (optional)

### Frontend Setup

```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at `http://localhost:3000`

### Backend Setup

```bash
cd backend
dotnet restore
dotnet run --project Marketplace.Api
```

The API will be available at `http://localhost:5000`

### Docker Setup

```bash
docker-compose up -d
```

This starts all services including PostgreSQL, Redis, and the application.

## Environment Variables

### Frontend (.env.local)
```env
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_WS_URL=ws://localhost:5000
```

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=marketplace;Username=postgres;Password=password",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters",
    "Issuer": "SocialMarketplace",
    "Audience": "SocialMarketplace"
  }
}
```

## API Documentation

The API documentation is available at `/swagger` when running in development mode.

### Key Endpoints

| Category | Endpoint | Description |
|----------|----------|-------------|
| Auth | POST /api/auth/login | User login |
| Auth | POST /api/auth/register | User registration |
| Users | GET /api/users/{id} | Get user profile |
| Connections | POST /api/connections/request | Send connection request |
| Follows | POST /api/follows | Follow a user/page |
| Messages | GET /api/messages/conversations | Get conversations |
| Search | GET /api/search | Unified search |
| Services | GET /api/services | Browse services |
| Projects | GET /api/projects | Browse projects |
| Tenders | GET /api/tenders | Browse tenders |

## Architecture

### Backend Architecture (Helix/Vertical Slice)

The backend follows a vertical slice architecture where features are organized by business capability:

```
Marketplace.Slices/
├── Users/
│   ├── UserRepository.cs
│   └── UserService.cs
├── Social/
│   ├── Connections/
│   ├── Follows/
│   ├── Messaging/
│   └── Search/
├── Marketplace/
│   ├── Services/
│   └── Products/
└── Projects/
    ├── Projects/
    └── Bids/
```

### Core Infrastructure

- **ConnectionFactory** - Manages read/write database connections
- **MultilevelCache** - L1 (Memory) + L2 (Redis) caching
- **QuantumRouter** - Probabilistic request routing
- **EntanglementManager** - Cross-slice transaction coordination
- **JobQueue** - Redis Streams-based job queue

### Real-time Features

SignalR hubs for real-time communication:
- **ChatHub** - Messaging and typing indicators
- **NotificationHub** - Push notifications
- **PresenceHub** - Online status tracking

## Internationalization

Supported languages:
- English (en)
- Urdu (ur)
- Arabic (ar)
- Chinese (zh)
- Spanish (es)

Add translations in `frontend/src/i18n/messages/`

## Feature Flags

Control feature visibility through the admin panel or configuration:

```typescript
// Enable for specific users
enableForUsers('new-feature', ['user-id-1', 'user-id-2'])

// Enable for percentage of users
enableForPercentage('new-feature', 50)

// Enable for specific roles
enableForRoles('new-feature', ['admin', 'seller'])
```

## Testing

### Frontend
```bash
npm run test
npm run test:e2e
```

### Backend
```bash
dotnet test
```

## Deployment

### Docker Deployment
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### Kubernetes
Kubernetes manifests are available in the `k8s/` directory.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License.
