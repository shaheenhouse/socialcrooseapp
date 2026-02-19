# Social Marketplace

A comprehensive social marketplace platform connecting freelancers, businesses, and government entities. Built with Next.js 15 and .NET 10 using modern best practices.

## Features

- **Multi-Role System**: Support for users, freelancers, stores, companies, agencies, and government entities
- **Marketplace**: Buy and sell products with inventory management
- **Services**: Offer and purchase services with package options
- **Projects**: Post projects and receive bids from freelancers
- **Government Tenders**: Transparent procurement process for government projects
- **Verified Skills**: Take skill tests and earn certificates
- **Secure Payments**: Escrow-based payments with milestone releases
- **Multi-Language**: Full internationalization support
- **Feature Flags**: Control feature visibility per user/role
- **Real-time**: WebSocket-based notifications and chat

## Tech Stack

### Frontend
- **Framework**: Next.js 15 (App Router)
- **UI**: Shadcn/ui + Tailwind CSS
- **State**: Zustand
- **Forms**: React Hook Form + Zod
- **Data Fetching**: TanStack Query
- **Animations**: Framer Motion

### Backend
- **Framework**: .NET 10 (Minimal APIs)
- **Database**: PostgreSQL with Entity Framework Core
- **Cache**: Redis
- **Real-time**: SignalR
- **Authentication**: JWT with refresh tokens
- **Observability**: OpenTelemetry + Prometheus

## Architecture

The backend follows a vertical slice architecture (Helix pattern):

```
backend/
├── Marketplace.Database/    # EF Core entities and configurations
├── Marketplace.Core/        # Infrastructure (caching, connections, etc.)
├── Marketplace.Slices/      # Business logic (repositories + services)
├── Marketplace.Orchestrator/# Workflows and job queues
├── Marketplace.Workers/     # Background workers
├── Marketplace.Api/         # REST API endpoints
└── Marketplace.Realtime/    # SignalR hubs
```

## Getting Started

### Prerequisites

- Node.js 20+
- .NET 10 SDK
- PostgreSQL 16+
- Redis 7+
- Docker (optional)

### Using Docker

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f
```

### Manual Setup

#### Backend

```bash
cd backend

# Restore packages
dotnet restore

# Update database (when migrations are added)
# dotnet ef database update --project Marketplace.Database

# Run the API
dotnet run --project Marketplace.Api
```

#### Frontend

```bash
cd frontend

# Install dependencies
npm install

# Run development server
npm run dev
```

## Environment Variables

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=social_marketplace;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters",
    "Issuer": "SocialMarketplace",
    "Audience": "SocialMarketplace"
  }
}
```

### Frontend (.env.local)

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_APP_NAME=Social Marketplace
```

## API Documentation

When running in development, Swagger UI is available at:
- http://localhost:5000/swagger

## Project Structure

### Database Entities

- **Identity**: Users, Roles, Permissions, Sessions
- **Feature Flags**: FeatureFlags, UserFeatureFlags, RoleFeatureFlags
- **Localization**: Languages, Translations
- **Marketplace**: Stores, Products, Services, Categories
- **Skills**: Skills, UserSkills, SkillTests, Certificates
- **Projects**: Projects, Bids, Milestones, Contracts
- **Tenders**: Tenders, TenderBids, TenderAwards
- **Orders**: Orders, OrderItems, Payments, Escrow
- **Communication**: ChatRooms, Messages, Notifications
- **Organizations**: Companies, Agencies

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## License

This project is licensed under the MIT License.
