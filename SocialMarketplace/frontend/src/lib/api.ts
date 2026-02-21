import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export const api = axios.create({
  baseURL: `${API_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000,
});

// --- Token Refresh Queue ---
// Prevents multiple concurrent refresh requests by queuing 401 retries
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

function processQueue(error: unknown, token: string | null = null) {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else if (token) resolve(token);
  });
  failedQueue = [];
}

function getStoredToken(key: string): string | null {
  if (typeof window === 'undefined') return null;
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}

function setStoredToken(key: string, value: string) {
  try {
    localStorage.setItem(key, value);
  } catch { /* storage full or unavailable */ }
}

function clearStoredTokens() {
  try {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  } catch { /* ignore */ }
}

// --- Background Token Refresh ---
// Proactively refreshes the token before it expires
let refreshTimer: ReturnType<typeof setTimeout> | null = null;

function parseJwtExp(token: string): number | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp ? payload.exp * 1000 : null;
  } catch {
    return null;
  }
}

async function performTokenRefresh(): Promise<string | null> {
  const refreshToken = getStoredToken('refreshToken');
  if (!refreshToken) return null;

  try {
    const response = await axios.post(`${API_URL}/api/auth/refresh`, { refreshToken });
    const { accessToken, refreshToken: newRefreshToken } = response.data;
    setStoredToken('accessToken', accessToken);
    setStoredToken('refreshToken', newRefreshToken);
    scheduleTokenRefresh(accessToken);
    return accessToken;
  } catch {
    clearStoredTokens();
    return null;
  }
}

export function scheduleTokenRefresh(token: string) {
  if (refreshTimer) clearTimeout(refreshTimer);
  const exp = parseJwtExp(token);
  if (!exp) return;

  // Refresh 2 minutes before expiry, minimum 10 seconds from now
  const refreshAt = Math.max(exp - Date.now() - 120_000, 10_000);
  refreshTimer = setTimeout(async () => {
    const newToken = await performTokenRefresh();
    if (!newToken && typeof window !== 'undefined') {
      window.location.href = '/auth/login';
    }
  }, refreshAt);
}

// Initialize background refresh on load
if (typeof window !== 'undefined') {
  const existingToken = getStoredToken('accessToken');
  if (existingToken) scheduleTokenRefresh(existingToken);
}

// Request interceptor
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = getStoredToken('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor with queued refresh
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const newToken = await performTokenRefresh();
        if (newToken) {
          processQueue(null, newToken);
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return api(originalRequest);
        }
        processQueue(new Error('Token refresh failed'));
        if (typeof window !== 'undefined') window.location.href = '/auth/login';
      } catch (refreshError) {
        processQueue(refreshError);
        clearStoredTokens();
        if (typeof window !== 'undefined') window.location.href = '/auth/login';
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

// Auth API
export const authApi = {
  login: (email: string, password: string) =>
    api.post('/auth/login', { email, password }),
  
  register: (data: {
    email: string;
    username: string;
    password: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
  }) => api.post('/auth/register', data),
  
  refresh: (refreshToken: string) =>
    api.post('/auth/refresh', { refreshToken }),
  
  logout: () => api.post('/auth/logout'),
};

// User API
export const userApi = {
  getMe: () => api.get('/users/me'),
  updateMe: (data: Record<string, unknown>) => api.patch('/users/me', data),
  getProfile: (userId: string) => api.get(`/users/${userId}/profile`),
  updateProfile: (data: Record<string, unknown>) => api.patch('/users/me/profile', data),
  getSkills: (userId: string) => api.get(`/users/${userId}/skills`),
  addSkill: (skillId: string, level: string, yearsOfExperience: number) =>
    api.post('/users/me/skills', { skillId, level, yearsOfExperience }),
  removeSkill: (skillId: string) => api.delete(`/users/me/skills/${skillId}`),
};

// Store API
export const storeApi = {
  getAll: (params?: { page?: number; pageSize?: number; search?: string }) =>
    api.get('/stores', { params }),
  getById: (id: string) => api.get(`/stores/${id}`),
  getBySlug: (slug: string) => api.get(`/stores/slug/${slug}`),
  create: (data: Record<string, unknown>) => api.post('/stores', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/stores/${id}`, data),
  getProducts: (id: string) => api.get(`/stores/${id}/products`),
  getMyStore: () => api.get('/stores/my'),
  getEmployees: (id: string) => api.get(`/stores/${id}/employees`),
  getAnalytics: (id: string) => api.get(`/stores/${id}/analytics`),
};

// Product API
export const productApi = {
  getAll: (params?: { 
    page?: number; 
    pageSize?: number; 
    search?: string;
    category?: string;
    minPrice?: number;
    maxPrice?: number;
  }) => api.get('/products', { params }),
  getById: (id: string) => api.get(`/products/${id}`),
  getBySlug: (slug: string) => api.get(`/products/slug/${slug}`),
  create: (data: Record<string, unknown>) => api.post('/products', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/products/${id}`, data),
  delete: (id: string) => api.delete(`/products/${id}`),
  getImages: (id: string) => api.get(`/products/${id}/images`),
  getVariants: (id: string) => api.get(`/products/${id}/variants`),
  getReviews: (id: string, params?: { page?: number; pageSize?: number }) =>
    api.get(`/products/${id}/reviews`, { params }),
};

// Project API
export const projectApi = {
  getAll: (params?: { page?: number; pageSize?: number; status?: string; category?: string }) =>
    api.get('/projects', { params }),
  getById: (id: string) => api.get(`/projects/${id}`),
  create: (data: Record<string, unknown>) => api.post('/projects', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/projects/${id}`, data),
  getBids: (id: string) => api.get(`/projects/${id}/bids`),
  submitBid: (id: string, data: Record<string, unknown>) => api.post(`/projects/${id}/bids`, data),
  getMilestones: (id: string) => api.get(`/projects/${id}/milestones`),
  createMilestone: (id: string, data: Record<string, unknown>) => api.post(`/projects/${id}/milestones`, data),
  getMyProjects: (params?: { page?: number; pageSize?: number }) =>
    api.get('/projects/my', { params }),
};

// Order API
export const orderApi = {
  getAll: (params?: { page?: number; pageSize?: number; status?: string }) =>
    api.get('/orders', { params }),
  getSales: (params?: { page?: number; pageSize?: number; status?: string }) =>
    api.get('/orders/sales', { params }),
  getById: (id: string) => api.get(`/orders/${id}`),
  getItems: (id: string) => api.get(`/orders/${id}/items`),
  create: (data: Record<string, unknown>) => api.post('/orders', data),
  updateStatus: (id: string, data: { status: string; notes?: string }) =>
    api.patch(`/orders/${id}/status`, data),
  cancel: (id: string, reason?: string) =>
    api.post(`/orders/${id}/cancel`, { reason }),
};

// Search API
export const searchApi = {
  searchAll: (params: {
    q: string;
    location?: string;
    industry?: string;
    minPrice?: number;
    maxPrice?: number;
    page?: number;
    pageSize?: number;
  }) => api.get('/search', { params }),
  searchUsers: (params: { q: string; location?: string; page?: number; pageSize?: number }) =>
    api.get('/search/users', { params }),
  searchServices: (params: { q: string; minPrice?: number; maxPrice?: number; page?: number; pageSize?: number }) =>
    api.get('/search/services', { params }),
  searchProjects: (params: { q: string; status?: string; page?: number; pageSize?: number }) =>
    api.get('/search/projects', { params }),
  searchCompanies: (params: { q: string; industry?: string; location?: string; page?: number; pageSize?: number }) =>
    api.get('/search/companies', { params }),
  searchPosts: (params: { q: string; page?: number; pageSize?: number }) =>
    api.get('/search/posts', { params }),
  getHistory: () => api.get('/search/history'),
  clearHistory: (id?: string) =>
    api.delete('/search/history', { params: id ? { id } : undefined }),
  getTrending: (type?: string) =>
    api.get('/search/trending', { params: type ? { type } : undefined }),
};

// Message API
export const messageApi = {
  getConversations: (params?: { page?: number; pageSize?: number; includeArchived?: boolean }) =>
    api.get('/messages/conversations', { params }),
  getConversation: (id: string) => api.get(`/messages/conversations/${id}`),
  getUnreadCount: () => api.get('/messages/unread-count'),
  getOrCreateDirect: (userId: string) =>
    api.post('/messages/conversations/direct', { userId }),
  createGroup: (data: { title: string; participantIds: string[] }) =>
    api.post('/messages/conversations/group', data),
  getMessages: (conversationId: string, params?: { page?: number; pageSize?: number; before?: string }) =>
    api.get(`/messages/conversations/${conversationId}/messages`, { params }),
  sendMessage: (conversationId: string, data: { content: string; type?: string; attachmentUrl?: string; replyToId?: string }) =>
    api.post(`/messages/conversations/${conversationId}/messages`, data),
  editMessage: (messageId: string, content: string) =>
    api.patch(`/messages/${messageId}`, { content }),
  deleteMessage: (messageId: string) => api.delete(`/messages/${messageId}`),
  markAsRead: (conversationId: string) =>
    api.post(`/messages/conversations/${conversationId}/read`),
  muteConversation: (conversationId: string, muted: boolean) =>
    api.patch(`/messages/conversations/${conversationId}/mute`, { muted }),
  archiveConversation: (conversationId: string, archived: boolean) =>
    api.patch(`/messages/conversations/${conversationId}/archive`, { archived }),
  leaveConversation: (conversationId: string) =>
    api.post(`/messages/conversations/${conversationId}/leave`),
};

// Follow API
export const followApi = {
  getStats: () => api.get('/follows/stats'),
  getUserStats: (userId: string) => api.get(`/follows/stats/${userId}`),
  getStatus: (targetId: string, targetType?: string) =>
    api.get(`/follows/status/${targetId}`, { params: targetType ? { targetType } : undefined }),
  getFollowers: (params?: { page?: number; pageSize?: number }) =>
    api.get('/follows/followers', { params }),
  getFollowing: (params?: { targetType?: string; page?: number; pageSize?: number }) =>
    api.get('/follows/following', { params }),
  follow: (targetId: string, targetType?: string) =>
    api.post('/follows', { targetId, targetType }),
  unfollow: (targetId: string, targetType?: string) =>
    api.delete('/follows', { data: { targetId, targetType } }),
  toggleNotifications: (targetId: string, targetType: string, enabled: boolean) =>
    api.patch('/follows/notifications', { targetId, targetType, enabled }),
};

// Connection API
export const connectionApi = {
  getAll: (params?: { page?: number; pageSize?: number }) =>
    api.get('/connections', { params }),
  getPending: (params?: { page?: number; pageSize?: number }) =>
    api.get('/connections/pending', { params }),
  getSent: (params?: { page?: number; pageSize?: number }) =>
    api.get('/connections/sent', { params }),
  getStats: () => api.get('/connections/stats'),
  getSuggestions: (limit?: number) =>
    api.get('/connections/suggestions', { params: limit ? { limit } : undefined }),
  getStatus: (userId: string) => api.get(`/connections/status/${userId}`),
  sendRequest: (userId: string, message?: string) =>
    api.post('/connections/request', { userId, message }),
  accept: (id: string) => api.post(`/connections/${id}/accept`),
  reject: (id: string) => api.post(`/connections/${id}/reject`),
  withdraw: (id: string) => api.delete(`/connections/${id}/withdraw`),
  remove: (id: string) => api.delete(`/connections/${id}`),
  block: (userId: string) => api.post('/connections/block', { userId }),
};

// Review API
export const reviewApi = {
  getById: (id: string) => api.get(`/reviews/${id}`),
  getByEntity: (entityType: string, entityId: string, params?: { page?: number; pageSize?: number }) =>
    api.get(`/reviews/${entityType}/${entityId}`, { params }),
  getStats: (entityType: string, entityId: string) =>
    api.get(`/reviews/${entityType}/${entityId}/stats`),
  create: (data: Record<string, unknown>) => api.post('/reviews', data),
  respond: (reviewId: string, content: string) =>
    api.post(`/reviews/${reviewId}/respond`, { content }),
  delete: (id: string) => api.delete(`/reviews/${id}`),
};

// Wallet API
export const walletApi = {
  getWallet: () => api.get('/wallet'),
  getTransactions: (params?: { page?: number; pageSize?: number; type?: string }) =>
    api.get('/wallet/transactions', { params }),
  getEscrows: (params?: { page?: number; pageSize?: number }) =>
    api.get('/wallet/escrows', { params }),
};

// Notification API
export const notificationApi = {
  getAll: (params?: { page?: number; pageSize?: number; unreadOnly?: boolean }) =>
    api.get('/notifications', { params }),
  getUnreadCount: () => api.get('/notifications/unread-count'),
  markAsRead: (id: string) => api.patch(`/notifications/${id}/read`),
  markAllAsRead: () => api.patch('/notifications/read-all'),
};

// Portfolio API
export const portfolioApi = {
  getPublic: (params?: { page?: number; pageSize?: number }) =>
    api.get('/portfolios/public', { params }),
  getBySlug: (slug: string) => api.get(`/portfolios/slug/${slug}`),
  getMe: () => api.get('/portfolios/me'),
  getById: (id: string) => api.get(`/portfolios/${id}`),
  create: (data: Record<string, unknown>) => api.post('/portfolios', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/portfolios/${id}`, data),
  delete: (id: string) => api.delete(`/portfolios/${id}`),
  uploadImage: (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post('/portfolios/upload-image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
};

// Resume API
export const resumeApi = {
  getMe: () => api.get('/resumes/me'),
  getById: (id: string) => api.get(`/resumes/${id}`),
  create: (data: Record<string, unknown>) => api.post('/resumes', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/resumes/${id}`, data),
  delete: (id: string) => api.delete(`/resumes/${id}`),
};

// Design API
export const designApi = {
  getMe: (params?: { page?: number; pageSize?: number }) =>
    api.get('/designs/me', { params }),
  getTemplates: (params?: { page?: number; pageSize?: number; category?: string }) =>
    api.get('/designs/templates', { params }),
  getById: (id: string) => api.get(`/designs/${id}`),
  create: (data: Record<string, unknown>) => api.post('/designs', data),
  duplicate: (id: string) => api.post(`/designs/${id}/duplicate`),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/designs/${id}`, data),
  delete: (id: string) => api.delete(`/designs/${id}`),
};

// Company API
export const companyApi = {
  getAll: (params?: { page?: number; pageSize?: number; search?: string; industry?: string }) =>
    api.get('/companies', { params }),
  getMy: () => api.get('/companies/my'),
  getById: (id: string) => api.get(`/companies/${id}`),
  getBySlug: (slug: string) => api.get(`/companies/slug/${slug}`),
  create: (data: Record<string, unknown>) => api.post('/companies', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/companies/${id}`, data),
  getEmployees: (companyId: string) => api.get(`/companies/${companyId}/employees`),
  addEmployee: (companyId: string, data: { userId: string; title: string; department?: string }) =>
    api.post(`/companies/${companyId}/employees`, data),
  removeEmployee: (companyId: string, userId: string) =>
    api.delete(`/companies/${companyId}/employees/${userId}`),
};

// Post/Feed API (LinkedIn-style posts)
export const postApi = {
  getFeed: (params?: { page?: number; pageSize?: number; type?: string }) =>
    api.get('/posts/feed', { params }),
  getById: (id: string) => api.get(`/posts/${id}`),
  getByUser: (userId: string, params?: { page?: number; pageSize?: number }) =>
    api.get(`/posts/user/${userId}`, { params }),
  create: (data: FormData) => api.post('/posts', data, { headers: { 'Content-Type': 'multipart/form-data' } }),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/posts/${id}`, data),
  delete: (id: string) => api.delete(`/posts/${id}`),
  react: (id: string, type: string) => api.post(`/posts/${id}/reactions`, { type }),
  removeReaction: (id: string) => api.delete(`/posts/${id}/reactions`),
  getComments: (id: string, params?: { page?: number; pageSize?: number }) =>
    api.get(`/posts/${id}/comments`, { params }),
  addComment: (id: string, data: { content: string; parentId?: string }) =>
    api.post(`/posts/${id}/comments`, data),
  share: (id: string, data?: { content?: string }) => api.post(`/posts/${id}/share`, data),
  report: (id: string, reason: string) => api.post(`/posts/${id}/report`, { reason }),
  bookmark: (id: string) => api.post(`/posts/${id}/bookmark`),
  removeBookmark: (id: string) => api.delete(`/posts/${id}/bookmark`),
  getBookmarks: (params?: { page?: number; pageSize?: number }) =>
    api.get('/posts/bookmarks', { params }),
  getTrending: (params?: { timeframe?: string; limit?: number }) =>
    api.get('/posts/trending', { params }),
};

// Khata/Ledger API (Pakistani-style credit/debit tracking)
export const khataApi = {
  getAll: (params?: { page?: number; pageSize?: number; search?: string; status?: string }) =>
    api.get('/khata', { params }),
  getById: (id: string) => api.get(`/khata/${id}`),
  create: (data: {
    partyName: string;
    partyPhone?: string;
    partyAddress?: string;
    type: 'customer' | 'supplier';
    openingBalance?: number;
  }) => api.post('/khata', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/khata/${id}`, data),
  delete: (id: string) => api.delete(`/khata/${id}`),
  getEntries: (id: string, params?: { page?: number; pageSize?: number; type?: string; from?: string; to?: string }) =>
    api.get(`/khata/${id}/entries`, { params }),
  addEntry: (id: string, data: {
    amount: number;
    type: 'credit' | 'debit';
    description: string;
    date?: string;
    attachmentUrl?: string;
  }) => api.post(`/khata/${id}/entries`, data),
  updateEntry: (id: string, entryId: string, data: Record<string, unknown>) =>
    api.patch(`/khata/${id}/entries/${entryId}`, data),
  deleteEntry: (id: string, entryId: string) =>
    api.delete(`/khata/${id}/entries/${entryId}`),
  getBalance: (id: string) => api.get(`/khata/${id}/balance`),
  getSummary: (params?: { from?: string; to?: string }) =>
    api.get('/khata/summary', { params }),
  export: (params?: { format?: 'pdf' | 'csv' | 'excel'; from?: string; to?: string }) =>
    api.get('/khata/export', { params, responseType: 'blob' }),
  sendReminder: (id: string) => api.post(`/khata/${id}/reminder`),
};

// Invoice API
export const invoiceApi = {
  getAll: (params?: { page?: number; pageSize?: number; status?: string; from?: string; to?: string }) =>
    api.get('/invoices', { params }),
  getById: (id: string) => api.get(`/invoices/${id}`),
  create: (data: Record<string, unknown>) => api.post('/invoices', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/invoices/${id}`, data),
  delete: (id: string) => api.delete(`/invoices/${id}`),
  send: (id: string) => api.post(`/invoices/${id}/send`),
  markPaid: (id: string, data?: { paymentMethod?: string; paidAt?: string }) =>
    api.post(`/invoices/${id}/mark-paid`, data),
  duplicate: (id: string) => api.post(`/invoices/${id}/duplicate`),
  getPublic: (token: string) => api.get(`/invoices/public/${token}`),
  export: (id: string, format?: 'pdf') =>
    api.get(`/invoices/${id}/export`, { params: { format }, responseType: 'blob' }),
  getSummary: (params?: { from?: string; to?: string }) =>
    api.get('/invoices/summary', { params }),
  getRecurring: () => api.get('/invoices/recurring'),
  createRecurring: (data: Record<string, unknown>) => api.post('/invoices/recurring', data),
};

// Inventory API
export const inventoryApi = {
  getAll: (params?: { page?: number; pageSize?: number; search?: string; category?: string; lowStock?: boolean }) =>
    api.get('/inventory', { params }),
  getById: (id: string) => api.get(`/inventory/${id}`),
  create: (data: Record<string, unknown>) => api.post('/inventory', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/inventory/${id}`, data),
  delete: (id: string) => api.delete(`/inventory/${id}`),
  adjustStock: (id: string, data: { quantity: number; type: 'add' | 'remove' | 'set'; reason: string }) =>
    api.post(`/inventory/${id}/adjust`, data),
  getMovements: (id: string, params?: { page?: number; pageSize?: number }) =>
    api.get(`/inventory/${id}/movements`, { params }),
  getLowStock: () => api.get('/inventory/low-stock'),
  getCategories: () => api.get('/inventory/categories'),
  export: (format?: 'csv' | 'excel') =>
    api.get('/inventory/export', { params: { format }, responseType: 'blob' }),
  importBulk: (data: FormData) =>
    api.post('/inventory/import', data, { headers: { 'Content-Type': 'multipart/form-data' } }),
  getValuation: () => api.get('/inventory/valuation'),
};

// Category API
export const categoryApi = {
  getAll: (params?: { type?: string; parentId?: string }) =>
    api.get('/categories', { params }),
  getById: (id: string) => api.get(`/categories/${id}`),
  getTree: (type?: string) => api.get('/categories/tree', { params: type ? { type } : undefined }),
  create: (data: { name: string; type: string; parentId?: string; icon?: string; description?: string }) =>
    api.post('/categories', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/categories/${id}`, data),
  delete: (id: string) => api.delete(`/categories/${id}`),
};

// Tender API (full implementation)
export const tenderApi = {
  getAll: (params?: { page?: number; pageSize?: number; status?: string; category?: string; search?: string }) =>
    api.get('/tenders', { params }),
  getById: (id: string) => api.get(`/tenders/${id}`),
  create: (data: Record<string, unknown>) => api.post('/tenders', data),
  update: (id: string, data: Record<string, unknown>) => api.patch(`/tenders/${id}`, data),
  delete: (id: string) => api.delete(`/tenders/${id}`),
  getBids: (id: string) => api.get(`/tenders/${id}/bids`),
  submitBid: (id: string, data: Record<string, unknown>) => api.post(`/tenders/${id}/bids`, data),
  evaluateBids: (id: string) => api.post(`/tenders/${id}/evaluate`),
  awardContract: (id: string, bidId: string) => api.post(`/tenders/${id}/award`, { bidId }),
  getDocuments: (id: string) => api.get(`/tenders/${id}/documents`),
  uploadDocument: (id: string, data: FormData) =>
    api.post(`/tenders/${id}/documents`, data, { headers: { 'Content-Type': 'multipart/form-data' } }),
  getMyTenders: (params?: { page?: number; pageSize?: number }) =>
    api.get('/tenders/my', { params }),
  getMyBids: (params?: { page?: number; pageSize?: number }) =>
    api.get('/tenders/my-bids', { params }),
};

// Cart API
export const cartApi = {
  get: () => api.get('/cart'),
  addItem: (data: { productId: string; quantity: number }) =>
    api.post('/cart/items', data),
  updateItem: (itemId: string, quantity: number) =>
    api.patch(`/cart/items/${itemId}`, { quantity }),
  removeItem: (itemId: string) => api.delete(`/cart/items/${itemId}`),
  clear: () => api.delete('/cart'),
};

// Wishlist API
export const wishlistApi = {
  getAll: () => api.get('/wishlist'),
  create: (name: string) => api.post('/wishlist', { name }),
  addItem: (wishlistId: string, data: { productId?: string; serviceId?: string }) =>
    api.post(`/wishlist/${wishlistId}/items`, data),
  removeItem: (wishlistId: string, itemId: string) =>
    api.delete(`/wishlist/${wishlistId}/items/${itemId}`),
  delete: (id: string) => api.delete(`/wishlist/${id}`),
};

// Discount API
export const discountApi = {
  getAll: (params?: { page?: number; pageSize?: number }) =>
    api.get('/discounts', { params }),
  getById: (id: string) => api.get(`/discounts/${id}`),
  create: (data: Record<string, unknown>) => api.post('/discounts', data),
  update: (id: string, data: Record<string, unknown>) =>
    api.patch(`/discounts/${id}`, data),
  delete: (id: string) => api.delete(`/discounts/${id}`),
  validate: (code: string, orderAmount: number) =>
    api.post('/discounts/validate', { code, orderAmount }),
};

// Analytics API
export const analyticsApi = {
  getDashboard: (params?: { from?: string; to?: string }) =>
    api.get('/analytics/dashboard', { params }),
  getRevenue: (params?: { from?: string; to?: string; granularity?: 'day' | 'week' | 'month' }) =>
    api.get('/analytics/revenue', { params }),
  getOrders: (params?: { from?: string; to?: string }) =>
    api.get('/analytics/orders', { params }),
  getCustomers: (params?: { from?: string; to?: string }) =>
    api.get('/analytics/customers', { params }),
  getProducts: (params?: { from?: string; to?: string; limit?: number }) =>
    api.get('/analytics/products', { params }),
  getTraffic: (params?: { from?: string; to?: string }) =>
    api.get('/analytics/traffic', { params }),
  getConversions: (params?: { from?: string; to?: string }) =>
    api.get('/analytics/conversions', { params }),
  exportReport: (type: string, params?: { from?: string; to?: string; format?: 'pdf' | 'csv' }) =>
    api.get(`/analytics/export/${type}`, { params, responseType: 'blob' }),
};
