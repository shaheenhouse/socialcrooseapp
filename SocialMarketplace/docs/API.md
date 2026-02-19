# API Documentation

## Base URL
```
Production: https://api.socialmarketplace.com
Development: http://localhost:5000
```

## Authentication

All authenticated endpoints require a JWT token in the Authorization header:
```
Authorization: Bearer <token>
```

### Auth Endpoints

#### POST /api/auth/register
Register a new user.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "username": "johndoe"
}
```

**Response:**
```json
{
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe"
  },
  "token": "jwt-token",
  "refreshToken": "refresh-token"
}
```

#### POST /api/auth/login
Login with email and password.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

#### POST /api/auth/refresh
Refresh access token.

**Request:**
```json
{
  "refreshToken": "refresh-token"
}
```

---

## Connections API

### GET /api/connections
Get user's connections.

**Query Parameters:**
- `status` (optional): Filter by status (pending, accepted, rejected)
- `page` (default: 1): Page number
- `pageSize` (default: 20): Items per page

**Response:**
```json
{
  "items": [
    {
      "id": "uuid",
      "otherUserId": "uuid",
      "status": "accepted",
      "message": null,
      "isOutgoing": false,
      "createdAt": "2024-02-26T10:00:00Z",
      "acceptedAt": "2024-02-26T12:00:00Z"
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

### GET /api/connections/pending
Get pending connection requests received.

### GET /api/connections/sent
Get sent connection requests.

### GET /api/connections/suggestions
Get connection suggestions (people you may know).

### GET /api/connections/status/{userId}
Check connection status with a specific user.

**Response:**
```json
{
  "isConnected": false,
  "isPending": true,
  "isBlocked": false,
  "connectionId": "uuid",
  "mutualConnectionsCount": 5,
  "isOutgoingRequest": true
}
```

### POST /api/connections/request
Send a connection request.

**Request:**
```json
{
  "userId": "uuid",
  "message": "Hi! I'd love to connect."
}
```

### POST /api/connections/{connectionId}/accept
Accept a connection request.

### POST /api/connections/{connectionId}/reject
Reject a connection request.

### POST /api/connections/{connectionId}/withdraw
Withdraw a sent connection request.

### DELETE /api/connections/{connectionId}
Remove a connection.

### POST /api/connections/block/{userId}
Block a user.

---

## Follows API

### GET /api/follows/stats
Get follow statistics for current user.

**Response:**
```json
{
  "followersCount": 1024,
  "followingCount": 156,
  "followingPagesCount": 12,
  "followingStoresCount": 8,
  "followingCompaniesCount": 5
}
```

### GET /api/follows/status
Check follow status with a target.

**Query Parameters:**
- `targetId`: Target user/page/store ID
- `targetType`: Type (User, Store, Company, Page, Project, Hashtag)

### GET /api/follows/followers
Get followers of a target.

### GET /api/follows/following
Get what current user is following.

### POST /api/follows
Follow a target.

**Request:**
```json
{
  "targetId": "uuid",
  "targetType": "User"
}
```

### DELETE /api/follows
Unfollow a target.

### PATCH /api/follows/notifications
Toggle notifications for a follow.

---

## Messages API

### GET /api/messages/conversations
Get all conversations.

**Query Parameters:**
- `page` (default: 1)
- `pageSize` (default: 20)
- `includeArchived` (default: false)

**Response:**
```json
[
  {
    "id": "uuid",
    "title": "John Doe",
    "type": "direct",
    "lastMessageAt": "2024-02-26T14:30:00Z",
    "unreadCount": 2,
    "isMuted": false,
    "isArchived": false
  }
]
```

### GET /api/messages/conversations/{conversationId}
Get a specific conversation.

### GET /api/messages/unread-count
Get total unread message count.

### POST /api/messages/conversations/direct/{userId}
Start or get a direct conversation with a user.

### POST /api/messages/conversations/group
Create a group conversation.

**Request:**
```json
{
  "title": "Project Alpha Team",
  "participantIds": ["uuid1", "uuid2", "uuid3"]
}
```

### GET /api/messages/conversations/{conversationId}/messages
Get messages in a conversation.

**Query Parameters:**
- `page` (default: 1)
- `pageSize` (default: 50)
- `beforeMessageId` (optional): For cursor-based pagination

### POST /api/messages/conversations/{conversationId}/messages
Send a message.

**Request:**
```json
{
  "content": "Hello!",
  "type": "text",
  "attachmentUrl": null,
  "replyToId": null
}
```

### PATCH /api/messages/{messageId}
Edit a message.

### DELETE /api/messages/{messageId}
Delete a message.

### POST /api/messages/conversations/{conversationId}/read
Mark conversation as read.

### PATCH /api/messages/conversations/{conversationId}/mute
Mute/unmute a conversation.

### PATCH /api/messages/conversations/{conversationId}/archive
Archive/unarchive a conversation.

### POST /api/messages/conversations/{conversationId}/leave
Leave a group conversation.

---

## Search API

### GET /api/search
Unified search across all types.

**Query Parameters:**
- `q` (required): Search query
- `location` (optional): Filter by location
- `industry` (optional): Filter by industry
- `minPrice` (optional): Minimum price
- `maxPrice` (optional): Maximum price
- `page` (default: 1)
- `pageSize` (default: 20)

**Response:**
```json
{
  "users": [...],
  "services": [...],
  "projects": [...],
  "companies": [...],
  "posts": [...]
}
```

### GET /api/search/users
Search users/people.

### GET /api/search/services
Search services.

### GET /api/search/projects
Search projects.

### GET /api/search/companies
Search companies.

### GET /api/search/posts
Search posts.

### GET /api/search/history
Get recent searches (authenticated).

### DELETE /api/search/history
Clear search history.

### GET /api/search/trending
Get trending searches.

---

## Services API

### GET /api/services
Browse services.

**Query Parameters:**
- `categoryId` (optional)
- `minPrice` (optional)
- `maxPrice` (optional)
- `minRating` (optional)
- `sellerId` (optional)
- `sortBy` (default: "relevance")
- `page` (default: 1)
- `pageSize` (default: 20)

### GET /api/services/{id}
Get service details.

### POST /api/services
Create a service (seller only).

### PUT /api/services/{id}
Update a service.

### DELETE /api/services/{id}
Delete a service.

---

## Projects API

### GET /api/projects
Browse projects.

### GET /api/projects/{id}
Get project details.

### POST /api/projects
Create a project.

### PUT /api/projects/{id}
Update a project.

### GET /api/projects/{id}/bids
Get bids for a project.

### POST /api/projects/{id}/bids
Submit a bid.

### POST /api/projects/{id}/bids/{bidId}/award
Award a bid.

---

## Tenders API

### GET /api/tenders
Browse tenders.

### GET /api/tenders/{id}
Get tender details.

### POST /api/tenders
Create a tender (authorized organizations).

### POST /api/tenders/{id}/bids
Submit a tender bid.

### POST /api/tenders/{id}/evaluate
Evaluate bids.

### POST /api/tenders/{id}/award
Award tender contract.

---

## Real-time (SignalR)

### Connection URL
```
wss://api.socialmarketplace.com/hubs/chat
wss://api.socialmarketplace.com/hubs/notifications
wss://api.socialmarketplace.com/hubs/presence
```

### ChatHub Methods

**Client -> Server:**
- `JoinRoom(roomId)` - Join a chat room
- `LeaveRoom(roomId)` - Leave a chat room
- `SendMessage(roomId, content, attachmentUrl?)` - Send a message
- `SendDirectMessage(recipientId, content, attachmentUrl?)` - Send DM
- `Typing(roomId)` - Send typing indicator
- `MarkAsRead(roomId, messageId)` - Mark messages as read

**Server -> Client:**
- `ReceiveMessage(message)` - New message received
- `UserTyping(roomId, userId)` - User is typing
- `MessageRead(roomId, userId, messageId)` - Message was read
- `UserOnline(userId)` - User came online
- `UserOffline(userId)` - User went offline

### PresenceHub Methods

**Client -> Server:**
- `UpdateStatus(status)` - Update online status
- `GetOnlineUsers()` - Get list of online users
- `CheckUsersOnline(userIds)` - Check if specific users are online
- `Heartbeat()` - Keep connection alive

**Server -> Client:**
- `UserConnected(userId, status)` - User connected
- `UserDisconnected(userId)` - User disconnected
- `UserStatusChanged(userId, status)` - User status changed

---

## Error Responses

All errors follow this format:
```json
{
  "error": "Error message",
  "code": "ERROR_CODE",
  "details": {}
}
```

### Common Error Codes
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `409` - Conflict
- `422` - Validation Error
- `429` - Rate Limited
- `500` - Internal Server Error

---

## Rate Limiting

API requests are rate limited:
- **Anonymous:** 60 requests/minute
- **Authenticated:** 300 requests/minute
- **Premium:** 1000 requests/minute

Headers:
```
X-RateLimit-Limit: 300
X-RateLimit-Remaining: 299
X-RateLimit-Reset: 1640000000
```
