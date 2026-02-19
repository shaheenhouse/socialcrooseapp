# Social Marketplace - Complete Feature Documentation

## Overview
A comprehensive platform combining **Marketplace**, **HR/Talent Management**, **Social Networking**, **Project Management**, and **Business Services** into one unified application.

---

## Architecture

| Layer | Technology |
|-------|-----------|
| **Frontend** | Next.js 15, React 19, TypeScript, Tailwind CSS, Radix UI, Zustand, TanStack Query |
| **Backend** | .NET 10, Minimal APIs, PostgreSQL 16, Redis 7, SignalR, Dapper + EF Core |
| **Real-time** | SignalR (Chat, Notifications, Presence) |
| **Caching** | Two-tier: L1 (MemoryCache 30s) + L2 (Redis 30min) |
| **Background Jobs** | Redis Streams with dedicated workers |
| **Observability** | Serilog, OpenTelemetry, Prometheus |

---

## Feature Matrix

### 1. Authentication & Authorization
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Email/Password Login | Done | Done | Production Ready |
| User Registration | Done | Done | Production Ready |
| JWT Token Management | Done | Done | Production Ready |
| Refresh Token Rotation | Done | Done | Production Ready |
| Token Revocation (Logout) | Done | Done | Production Ready |
| Role-Based Access Control | Done | Done | Production Ready |
| Password Change | Done | Done | Production Ready |
| Account Lockout | Schema Ready | - | Planned |
| Two-Factor Authentication | Schema Ready | - | Planned |
| Email Verification | Schema Ready | - | Planned |
| Phone Verification | Schema Ready | - | Planned |
| OAuth (Google, GitHub, LinkedIn) | - | - | Planned |

### 2. User & Profile Management
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| User CRUD | Done | Done | Production Ready |
| User Profile (Headline, About, etc.) | Done | Done | Production Ready |
| Avatar Upload | Done | Done | Production Ready |
| Skills Management | Done | Done | Production Ready |
| Portfolio Management | Done | Done | Production Ready |
| Social Links (LinkedIn, GitHub, etc.) | Done | Done | Production Ready |
| Work Experience | Done | Done | Production Ready |
| Education | Done | Done | Production Ready |
| Hourly Rate & Availability | Done | Done | Production Ready |
| User Search & Discovery | Done | Done | Production Ready |
| Profile Completeness Score | Done | Done | Production Ready |

### 3. Marketplace - Stores
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Store Creation & Setup | Done | Done | Production Ready |
| Store Profile (Logo, Banner, Description) | Done | Done | Production Ready |
| Store Settings & Policies | Done | Done | Production Ready |
| Store Analytics | Done | Done | Production Ready |
| Store Verification | Done | Done | Production Ready |
| Store Employees | Done | Done | Production Ready |
| Store Categories | Done | Done | Production Ready |
| Business Hours | Done | Done | Production Ready |
| Commission Management | Done | Done | Production Ready |

### 4. Marketplace - Products
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Product CRUD | Done | Done | Production Ready |
| Product Variants | Done | Done | Production Ready |
| Product Images | Done | Done | Production Ready |
| Inventory Tracking | Done | Done | Production Ready |
| Product Categories & Tags | Done | Done | Production Ready |
| Digital Products | Done | Done | Production Ready |
| Product SEO | Done | Done | Production Ready |
| Product Reviews | Done | Done | Production Ready |
| Wishlist | Done | Done | Production Ready |
| Product Search & Filters | Done | Done | Production Ready |

### 5. Marketplace - Services
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Service Listings | Done | Done | Production Ready |
| Service Packages (Basic/Standard/Premium) | Done | Done | Production Ready |
| Service Images | Done | Done | Production Ready |
| Fixed/Hourly/Daily Pricing | Done | Done | Production Ready |
| Service Requirements | Done | Done | Production Ready |
| Service FAQ | Done | Done | Production Ready |
| Service Reviews | Done | Done | Production Ready |
| Service Search & Filters | Done | Done | Production Ready |

### 6. Orders & Transactions
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Order Placement | Done | Done | Production Ready |
| Order Status Tracking | Done | Done | Production Ready |
| Order Cancellation | Done | Done | Production Ready |
| Order History | Done | Done | Production Ready |
| Shipping Information | Done | Done | Production Ready |
| Discount Codes | Done | Done | Production Ready |
| Commission Calculation | Done | Done | Production Ready |
| Order Timeline | Done | Done | Production Ready |

### 7. Projects & Freelancing
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Project Posting | Done | Done | Production Ready |
| Project Browsing & Search | Done | Done | Production Ready |
| Bid Submission | Done | Done | Production Ready |
| Bid Management | Done | Done | Production Ready |
| Project Milestones | Done | Done | Production Ready |
| Project Contracts | Done | Done | Production Ready |
| Project Status Tracking | Done | Done | Production Ready |
| Project Chat Rooms | Done | Done | Production Ready |

### 8. Government Tenders
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Tender Browsing | Done | Done | Production Ready |
| Tender Bid Submission | Done | Done | Production Ready |
| Tender Documents | Done | Done | Production Ready |
| Tender Awards | Done | Done | Production Ready |
| Tender Evaluation | Done | Done | Production Ready |

### 9. Payments & Wallet
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Wallet Management | Done | Done | Production Ready |
| Transaction History | Done | Done | Production Ready |
| Escrow Payments | Done | Done | Production Ready |
| Escrow Release/Refund | Done | Done | Production Ready |
| Payout to Sellers | Done | Done | Production Ready |
| Multiple Payment Methods | Done | Done | Production Ready |
| Payment Gateway Integration | Schema Ready | UI Ready | Planned |

### 10. Social Networking
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Follow/Unfollow Users | Done | Done | Production Ready |
| Connection Requests (LinkedIn-style) | Done | Done | Production Ready |
| Connection Suggestions | Done | Done | Production Ready |
| Mutual Connections | Done | Done | Production Ready |
| Block Users | Done | Done | Production Ready |
| Follow Stores/Companies/Pages | Done | Done | Production Ready |
| News Feed / Posts | Done | Done | Production Ready |
| Post Reactions | Done | Done | Production Ready |

### 11. Messaging & Chat
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Direct Messaging | Done | Done | Production Ready |
| Group Conversations | Done | Done | Production Ready |
| Real-time Chat (SignalR) | Done | Done | Production Ready |
| File Attachments | Done | Done | Production Ready |
| Read Receipts | Done | Done | Production Ready |
| Typing Indicators | Done | Done | Production Ready |
| Message Editing/Deletion | Done | Done | Production Ready |
| Mute/Archive Conversations | Done | Done | Production Ready |
| Unread Count | Done | Done | Production Ready |

### 12. Search (LinkedIn-Level)
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Unified Search (All Types) | Done | Done | Production Ready |
| People Search | Done | Done | Production Ready |
| Service Search | Done | Done | Production Ready |
| Project Search | Done | Done | Production Ready |
| Company Search | Done | Done | Production Ready |
| Post Search | Done | Done | Production Ready |
| Job Search | Done | Done | Production Ready |
| Search History | Done | Done | Production Ready |
| Trending Searches | Done | Done | Production Ready |
| Search Click Tracking | Done | Done | Production Ready |
| Advanced Filters | Done | Done | Production Ready |
| Command Palette (Cmd+K) | - | Done | Production Ready |
| Full-Text PostgreSQL Search | Done | Done | Production Ready |

### 13. Skills & Certifications
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Skill Assessment Tests | Done | Done | Production Ready |
| Multiple Question Types | Done | Done | Production Ready |
| Test Timer & Proctoring | Done | Done | Production Ready |
| Score Calculation | Done | Done | Production Ready |
| Certificates | Done | Done | Production Ready |
| Skill Endorsements | Done | Done | Production Ready |
| Skill Verification | Done | Done | Production Ready |

### 14. HR & Recruitment System
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Company Management | Done | Done | Production Ready |
| Company Employees | Done | Done | Production Ready |
| Job Posting | Done | Done | Production Ready |
| Job Search | Done | Done | Production Ready |
| Job Applications | Done | Done | Production Ready |
| Resume/CV Builder | Done | Done | Production Ready |
| Resume PDF Export | Done | Done | Production Ready |
| Employee Roles & Permissions | Done | Done | Production Ready |
| Department Management | Done | Done | Production Ready |
| Agency Management | Done | Done | Production Ready |

### 15. Reviews & Ratings
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Product Reviews | Done | Done | Production Ready |
| Service Reviews | Done | Done | Production Ready |
| Store Reviews | Done | Done | Production Ready |
| Seller Reviews | Done | Done | Production Ready |
| Review Responses | Done | Done | Production Ready |
| Rating Breakdown | Done | Done | Production Ready |
| Verified Purchase Badge | Done | Done | Production Ready |

### 16. Notifications
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| In-App Notifications | Done | Done | Production Ready |
| Real-time Push (SignalR) | Done | Done | Production Ready |
| Notification Types | Done | Done | Production Ready |
| Mark as Read | Done | Done | Production Ready |
| Notification Preferences | Done | Done | Production Ready |
| Email Notifications | Worker Ready | - | Planned |
| SMS Notifications | Worker Ready | - | Planned |

### 17. Internationalization
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Multi-language Support | Done | Done | Production Ready |
| RTL Support (Arabic, Urdu) | Done | Done | Production Ready |
| Language Switching | Done | Done | Production Ready |
| Translation Management | Done | - | Production Ready |

### 18. Admin & Platform
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Feature Flags | Done | Done | Production Ready |
| Audit Logging | Done | - | Production Ready |
| Role-Based Dashboards | Done | Done | Production Ready |
| Platform Analytics | Done | Done | Production Ready |

### 19. Cart & Checkout
| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| Shopping Cart | Done | Done | Production Ready |
| Guest Cart (Session-based) | Done | Done | Production Ready |
| Saved for Later | Done | Done | Production Ready |
| Discount Application | Done | Done | Production Ready |
| Tax & Shipping Calculation | Done | Done | Production Ready |
| Checkout Flow | Done | Done | Production Ready |

---

## Database Entities (74 Total)

### Identity: User, UserProfile, UserSession, Role, Permission, UserRole, RolePermission
### Features: FeatureFlag, UserFeatureFlag, RoleFeatureFlag
### Localization: Language, Translation
### Marketplace: Store, StoreEmployee, StoreCategory, Category, Product, ProductImage, ProductVariant, Service, ServicePackage, ServiceImage
### Skills: Skill, UserSkill, SkillTest, SkillTestQuestion, SkillTestAttempt, SkillTestAnswer, SkillCertificate
### Projects: Project, ProjectBid, ProjectMilestone, ProjectContract
### Tenders: Tender, TenderBid, TenderDocument, TenderAward
### Companies: Company, CompanyEmployee, Agency, AgencyMember
### Orders: Order, OrderItem, Cart, CartItem, Wishlist, WishlistItem
### Payments: Payment, PaymentGateway, Escrow, EscrowRelease, Wallet, Transaction, Payout
### Reviews: Review, ReviewResponse
### Social: Post, PostReaction, Page, Connection, Follow, Conversation, ConversationParticipant, Message, SearchHistory, TrendingSearch
### Chat: ChatRoom, ChatParticipant, ChatMessage, ChatMessageRead
### Discounts: Discount, DiscountUsage
### System: Notification, AuditLog, OutboxMessage

---

## API Endpoints

### Auth: POST /api/auth/login, /register, /refresh, /logout
### Users: GET/PATCH /api/users/me, GET /api/users/{id}, GET /api/users, profiles, skills
### Stores: Full CRUD /api/stores, employees, categories, analytics
### Products: Full CRUD /api/products, images, variants, reviews
### Services: Full CRUD /api/services, packages, images
### Orders: Full CRUD /api/orders, status updates, cancellation, payments
### Projects: Full CRUD /api/projects, bids, milestones, contracts
### Tenders: Full CRUD /api/tenders, bids, documents, awards
### Companies: Full CRUD /api/companies, employees, departments
### HR/Jobs: Full CRUD /api/jobs, applications, resumes
### Reviews: Full CRUD /api/reviews, responses
### Wallet: GET /api/wallet, transactions, payouts
### Cart: Full CRUD /api/cart, items
### Search: GET /api/search (unified), /users, /services, /projects, /companies, /posts, /jobs
### Messages: Full CRUD /api/messages, conversations
### Follows: Full CRUD /api/follows
### Connections: Full CRUD /api/connections
### Notifications: GET /api/notifications, mark read
### Skills: GET /api/skills, tests, attempts, certificates

---

## Seed Data Categories
- **Users**: 25+ diverse users (freelancers, clients, store owners, admins, HR managers)
- **Skills**: 50+ skills across categories (Programming, Design, Marketing, Finance, etc.)
- **Categories**: Full taxonomy for Products, Services, Skills, Projects
- **Stores**: 10+ stores with products and services
- **Products**: 50+ products with variants and images
- **Services**: 30+ services with packages
- **Projects**: 20+ projects with bids and milestones
- **Companies**: 10+ companies with employees
- **Jobs**: 15+ job postings
- **Reviews**: 100+ reviews across entities
- **Connections**: Network of connections between users
- **Posts**: Social feed content

---

## Technical Notes

### Search Implementation
- PostgreSQL full-text search with `tsvector`/`tsquery`
- Weighted ranking (title > description > tags)
- Trigram similarity for fuzzy matching
- Unified search across all entity types
- Search history and trending analytics

### Security
- BCrypt password hashing (work factor 12)
- JWT with RSA256 signing
- Refresh token rotation
- Account lockout after failed attempts
- Rate limiting on auth endpoints
- CORS configuration
- Input validation with FluentValidation

### Performance
- Two-tier caching (L1: Memory, L2: Redis)
- Read/Write connection separation
- Dapper for read-heavy queries
- EF Core for write operations
- Pagination on all list endpoints
- Background job processing

### Real-time
- SignalR for chat, notifications, presence
- WebSocket fallback
- User-specific notification channels
- Typing indicators and online status
