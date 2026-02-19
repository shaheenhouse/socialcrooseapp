# Database Schema Documentation

## Overview

The Social Marketplace Platform uses PostgreSQL as its primary database. The schema is designed for scalability, performance, and maintainability.

## Core Tables

### Users

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    username VARCHAR(100) UNIQUE,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    display_name VARCHAR(200),
    headline VARCHAR(500),
    bio TEXT,
    avatar_url VARCHAR(500),
    cover_image_url VARCHAR(500),
    location VARCHAR(200),
    phone VARCHAR(50),
    date_of_birth DATE,
    is_verified BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    email_verified BOOLEAN DEFAULT FALSE,
    two_factor_enabled BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    last_login_at TIMESTAMP
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_is_active ON users(is_active);
```

### Roles & Permissions

```sql
CREATE TABLE roles (
    id UUID PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(500),
    is_system BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE permissions (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(500),
    resource VARCHAR(100),
    action VARCHAR(50)
);

CREATE TABLE user_roles (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    role_id UUID NOT NULL REFERENCES roles(id),
    assigned_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, role_id)
);

CREATE TABLE role_permissions (
    id UUID PRIMARY KEY,
    role_id UUID NOT NULL REFERENCES roles(id),
    permission_id UUID NOT NULL REFERENCES permissions(id),
    UNIQUE(role_id, permission_id)
);
```

## Social Tables

### Connections

```sql
CREATE TABLE connections (
    id UUID PRIMARY KEY,
    requester_id UUID NOT NULL REFERENCES users(id),
    addressee_id UUID NOT NULL REFERENCES users(id),
    status SMALLINT NOT NULL DEFAULT 0, -- 0: pending, 1: accepted, 2: rejected, 3: blocked, 4: withdrawn
    message VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    accepted_at TIMESTAMP,
    rejected_at TIMESTAMP,
    blocked_at TIMESTAMP,
    UNIQUE(requester_id, addressee_id)
);

CREATE INDEX idx_connections_requester ON connections(requester_id);
CREATE INDEX idx_connections_addressee ON connections(addressee_id);
CREATE INDEX idx_connections_status ON connections(status);
```

### Follows

```sql
CREATE TABLE follows (
    id UUID PRIMARY KEY,
    follower_id UUID NOT NULL REFERENCES users(id),
    following_id UUID NOT NULL,
    target_type SMALLINT NOT NULL, -- 0: User, 1: Store, 2: Company, 3: Page, 4: Project, 5: Hashtag
    notifications_enabled BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(follower_id, following_id, target_type)
);

CREATE INDEX idx_follows_follower ON follows(follower_id);
CREATE INDEX idx_follows_following ON follows(following_id, target_type);
```

### Pages (Company/Organization Pages)

```sql
CREATE TABLE pages (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(200) NOT NULL UNIQUE,
    description TEXT,
    tagline VARCHAR(500),
    logo_url VARCHAR(500),
    cover_image_url VARCHAR(500),
    website VARCHAR(500),
    industry VARCHAR(200),
    company_size VARCHAR(50),
    headquarters VARCHAR(200),
    founded_year INTEGER,
    type SMALLINT NOT NULL, -- 0: Company, 1: Organization, 2: Educational, 3: Government, 4: Community, 5: Brand
    owner_id UUID NOT NULL REFERENCES users(id),
    is_verified BOOLEAN DEFAULT FALSE,
    follower_count INTEGER DEFAULT 0,
    employee_count INTEGER DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    is_active BOOLEAN DEFAULT TRUE,
    linkedin_url VARCHAR(500),
    twitter_url VARCHAR(500),
    facebook_url VARCHAR(500),
    instagram_url VARCHAR(500)
);

CREATE INDEX idx_pages_slug ON pages(slug);
CREATE INDEX idx_pages_owner ON pages(owner_id);
CREATE INDEX idx_pages_type ON pages(type);
```

### Posts

```sql
CREATE TABLE posts (
    id UUID PRIMARY KEY,
    author_id UUID NOT NULL REFERENCES users(id),
    page_id UUID REFERENCES pages(id),
    content TEXT NOT NULL,
    type SMALLINT NOT NULL DEFAULT 0, -- 0: Text, 1: Image, 2: Video, 3: Article, 4: Poll, 5: Event, 6: Job, 7: Document, 8: Celebration
    visibility SMALLINT DEFAULT 0, -- 0: Public, 1: Connections Only, 2: Private
    media_urls JSONB,
    link_preview JSONB,
    shared_post_id UUID REFERENCES posts(id),
    parent_post_id UUID REFERENCES posts(id),
    like_count INTEGER DEFAULT 0,
    comment_count INTEGER DEFAULT 0,
    share_count INTEGER DEFAULT 0,
    view_count INTEGER DEFAULT 0,
    is_edited BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    edited_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

CREATE INDEX idx_posts_author ON posts(author_id);
CREATE INDEX idx_posts_page ON posts(page_id);
CREATE INDEX idx_posts_parent ON posts(parent_post_id);
CREATE INDEX idx_posts_created ON posts(created_at);
```

### Post Reactions

```sql
CREATE TABLE post_reactions (
    id UUID PRIMARY KEY,
    post_id UUID NOT NULL REFERENCES posts(id),
    user_id UUID NOT NULL REFERENCES users(id),
    type SMALLINT NOT NULL, -- 0: Like, 1: Love, 2: Celebrate, 3: Support, 4: Insightful, 5: Funny
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(post_id, user_id)
);

CREATE INDEX idx_post_reactions_post ON post_reactions(post_id);
CREATE INDEX idx_post_reactions_user ON post_reactions(user_id);
```

## Messaging Tables

### Conversations

```sql
CREATE TABLE conversations (
    id UUID PRIMARY KEY,
    title VARCHAR(200),
    type SMALLINT NOT NULL DEFAULT 0, -- 0: Direct, 1: Group, 2: Project, 3: Order, 4: Support
    project_id UUID,
    order_id UUID,
    last_message_id UUID,
    last_message_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    is_active BOOLEAN DEFAULT TRUE
);

CREATE INDEX idx_conversations_project ON conversations(project_id);
CREATE INDEX idx_conversations_order ON conversations(order_id);
CREATE INDEX idx_conversations_last_message ON conversations(last_message_at);
```

### Conversation Participants

```sql
CREATE TABLE conversation_participants (
    id UUID PRIMARY KEY,
    conversation_id UUID NOT NULL REFERENCES conversations(id),
    user_id UUID NOT NULL REFERENCES users(id),
    role SMALLINT DEFAULT 0, -- 0: Member, 1: Admin, 2: Owner
    last_read_message_id UUID,
    last_read_at TIMESTAMP,
    unread_count INTEGER DEFAULT 0,
    is_muted BOOLEAN DEFAULT FALSE,
    is_archived BOOLEAN DEFAULT FALSE,
    joined_at TIMESTAMP NOT NULL DEFAULT NOW(),
    left_at TIMESTAMP,
    UNIQUE(conversation_id, user_id)
);

CREATE INDEX idx_conv_participants_conversation ON conversation_participants(conversation_id);
CREATE INDEX idx_conv_participants_user ON conversation_participants(user_id);
```

### Messages

```sql
CREATE TABLE messages (
    id UUID PRIMARY KEY,
    conversation_id UUID NOT NULL REFERENCES conversations(id),
    sender_id UUID NOT NULL REFERENCES users(id),
    content TEXT NOT NULL,
    type SMALLINT DEFAULT 0, -- 0: Text, 1: Image, 2: File, 3: Audio, 4: Video, 5: System, 6: Project, 7: Order
    attachment_url VARCHAR(500),
    attachment_name VARCHAR(200),
    attachment_size BIGINT,
    reply_to_id UUID REFERENCES messages(id),
    is_edited BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    edited_at TIMESTAMP,
    deleted_at TIMESTAMP
);

CREATE INDEX idx_messages_conversation ON messages(conversation_id);
CREATE INDEX idx_messages_sender ON messages(sender_id);
CREATE INDEX idx_messages_created ON messages(created_at);
```

## Search Tables

### Search History

```sql
CREATE TABLE search_history (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    query VARCHAR(500) NOT NULL,
    type SMALLINT NOT NULL, -- 0: All, 1: People, 2: Services, 3: Projects, 4: Companies, 5: Jobs, 6: Posts, 7: Tenders
    filters JSONB,
    result_count INTEGER,
    clicked_result_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_search_history_user ON search_history(user_id);
CREATE INDEX idx_search_history_query ON search_history(query);
CREATE INDEX idx_search_history_created ON search_history(created_at);
```

### Trending Searches

```sql
CREATE TABLE trending_searches (
    id UUID PRIMARY KEY,
    query VARCHAR(500) NOT NULL,
    type SMALLINT NOT NULL,
    search_count INTEGER NOT NULL,
    period_start TIMESTAMP NOT NULL,
    period_end TIMESTAMP NOT NULL,
    rank INTEGER
);

CREATE INDEX idx_trending_searches_period ON trending_searches(period_start, period_end, type);
CREATE INDEX idx_trending_searches_rank ON trending_searches(rank);
```

## Marketplace Tables

### Services

```sql
CREATE TABLE services (
    id UUID PRIMARY KEY,
    seller_id UUID NOT NULL REFERENCES users(id),
    title VARCHAR(200) NOT NULL,
    description TEXT,
    category_id UUID,
    price DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    delivery_days INTEGER,
    revision_count INTEGER DEFAULT 1,
    rating DECIMAL(3,2) DEFAULT 0,
    review_count INTEGER DEFAULT 0,
    order_count INTEGER DEFAULT 0,
    view_count INTEGER DEFAULT 0,
    is_featured BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_services_seller ON services(seller_id);
CREATE INDEX idx_services_category ON services(category_id);
CREATE INDEX idx_services_rating ON services(rating);
CREATE INDEX idx_services_is_active ON services(is_active);
```

### Orders

```sql
CREATE TABLE orders (
    id UUID PRIMARY KEY,
    buyer_id UUID NOT NULL REFERENCES users(id),
    seller_id UUID NOT NULL REFERENCES users(id),
    service_id UUID REFERENCES services(id),
    status VARCHAR(50) NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    delivery_date DATE,
    completed_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_orders_buyer ON orders(buyer_id);
CREATE INDEX idx_orders_seller ON orders(seller_id);
CREATE INDEX idx_orders_status ON orders(status);
```

## Project Tables

### Projects

```sql
CREATE TABLE projects (
    id UUID PRIMARY KEY,
    client_id UUID NOT NULL REFERENCES users(id),
    title VARCHAR(200) NOT NULL,
    description TEXT,
    category_id UUID,
    budget_min DECIMAL(18,2),
    budget_max DECIMAL(18,2),
    currency VARCHAR(3) DEFAULT 'USD',
    status VARCHAR(50) NOT NULL DEFAULT 'open',
    deadline DATE,
    bid_count INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_projects_client ON projects(client_id);
CREATE INDEX idx_projects_status ON projects(status);
CREATE INDEX idx_projects_created ON projects(created_at);
```

### Bids

```sql
CREATE TABLE bids (
    id UUID PRIMARY KEY,
    project_id UUID NOT NULL REFERENCES projects(id),
    bidder_id UUID NOT NULL REFERENCES users(id),
    amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    delivery_days INTEGER,
    proposal TEXT,
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    is_awarded BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_bids_project ON bids(project_id);
CREATE INDEX idx_bids_bidder ON bids(bidder_id);
CREATE INDEX idx_bids_is_awarded ON bids(is_awarded);
```

## Tender Tables

### Tenders

```sql
CREATE TABLE tenders (
    id UUID PRIMARY KEY,
    organization_id UUID NOT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    category_id UUID,
    budget DECIMAL(18,2),
    currency VARCHAR(3) DEFAULT 'USD',
    status VARCHAR(50) NOT NULL DEFAULT 'draft',
    submission_deadline TIMESTAMP NOT NULL,
    evaluation_criteria JSONB,
    documents JSONB,
    bid_count INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    published_at TIMESTAMP,
    awarded_at TIMESTAMP
);

CREATE INDEX idx_tenders_organization ON tenders(organization_id);
CREATE INDEX idx_tenders_status ON tenders(status);
CREATE INDEX idx_tenders_deadline ON tenders(submission_deadline);
```

### Tender Bids

```sql
CREATE TABLE tender_bids (
    id UUID PRIMARY KEY,
    tender_id UUID NOT NULL REFERENCES tenders(id),
    bidder_id UUID NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    technical_proposal TEXT,
    financial_proposal TEXT,
    documents JSONB,
    status VARCHAR(50) NOT NULL DEFAULT 'submitted',
    technical_score DECIMAL(5,2),
    financial_score DECIMAL(5,2),
    total_score DECIMAL(5,2),
    is_awarded BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_tender_bids_tender ON tender_bids(tender_id);
CREATE INDEX idx_tender_bids_bidder ON tender_bids(bidder_id);
CREATE INDEX idx_tender_bids_is_awarded ON tender_bids(is_awarded);
```

## Payment Tables

### Wallets

```sql
CREATE TABLE wallets (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL UNIQUE REFERENCES users(id),
    balance DECIMAL(18,2) DEFAULT 0,
    currency VARCHAR(3) DEFAULT 'USD',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_wallets_user ON wallets(user_id);
```

### Transactions

```sql
CREATE TABLE transactions (
    id UUID PRIMARY KEY,
    wallet_id UUID NOT NULL REFERENCES wallets(id),
    type VARCHAR(50) NOT NULL, -- deposit, withdrawal, payment, refund, escrow_hold, escrow_release
    amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    status VARCHAR(50) NOT NULL,
    reference_id UUID,
    reference_type VARCHAR(50),
    description VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_transactions_wallet ON transactions(wallet_id);
CREATE INDEX idx_transactions_type ON transactions(type);
CREATE INDEX idx_transactions_created ON transactions(created_at);
```

### Escrow

```sql
CREATE TABLE escrow (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL,
    buyer_id UUID NOT NULL REFERENCES users(id),
    seller_id UUID NOT NULL REFERENCES users(id),
    amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    funded_at TIMESTAMP,
    released_at TIMESTAMP,
    refunded_at TIMESTAMP
);

CREATE INDEX idx_escrow_order ON escrow(order_id);
CREATE INDEX idx_escrow_buyer ON escrow(buyer_id);
CREATE INDEX idx_escrow_seller ON escrow(seller_id);
CREATE INDEX idx_escrow_status ON escrow(status);
```

## Skills & Certifications

### Skills

```sql
CREATE TABLE skills (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    category VARCHAR(100),
    description VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE user_skills (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    skill_id UUID NOT NULL REFERENCES skills(id),
    proficiency_level SMALLINT, -- 1-5
    years_experience DECIMAL(4,1),
    is_verified BOOLEAN DEFAULT FALSE,
    verified_at TIMESTAMP,
    UNIQUE(user_id, skill_id)
);

CREATE INDEX idx_user_skills_user ON user_skills(user_id);
CREATE INDEX idx_user_skills_skill ON user_skills(skill_id);
```

### Certifications

```sql
CREATE TABLE certifications (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    name VARCHAR(200) NOT NULL,
    issuing_organization VARCHAR(200),
    issue_date DATE,
    expiry_date DATE,
    credential_id VARCHAR(100),
    credential_url VARCHAR(500),
    is_verified BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_certifications_user ON certifications(user_id);
```

## Performance Indexes

```sql
-- Full-text search indexes
CREATE INDEX idx_users_search ON users USING gin(to_tsvector('english', display_name || ' ' || COALESCE(headline, '')));
CREATE INDEX idx_services_search ON services USING gin(to_tsvector('english', title || ' ' || COALESCE(description, '')));
CREATE INDEX idx_projects_search ON projects USING gin(to_tsvector('english', title || ' ' || COALESCE(description, '')));
CREATE INDEX idx_posts_search ON posts USING gin(to_tsvector('english', content));

-- Composite indexes for common queries
CREATE INDEX idx_connections_user_status ON connections(requester_id, status) WHERE status = 1;
CREATE INDEX idx_follows_user_type ON follows(follower_id, target_type);
CREATE INDEX idx_messages_conv_created ON messages(conversation_id, created_at DESC);
```

## Data Retention

```sql
-- Archive old messages (keep last 2 years)
CREATE TABLE messages_archive (LIKE messages INCLUDING ALL);

-- Archive old search history (keep last 30 days)
CREATE TABLE search_history_archive (LIKE search_history INCLUDING ALL);
```
