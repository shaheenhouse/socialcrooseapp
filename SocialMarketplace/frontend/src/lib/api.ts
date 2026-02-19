import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export const api = axios.create({
  baseURL: `${API_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = typeof window !== 'undefined' ? localStorage.getItem('accessToken') : null;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    
    // If 401 and we haven't tried to refresh yet
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        const refreshToken = localStorage.getItem('refreshToken');
        if (refreshToken) {
          const response = await axios.post(`${API_URL}/api/auth/refresh`, {
            refreshToken,
          });
          
          const { accessToken, refreshToken: newRefreshToken } = response.data;
          localStorage.setItem('accessToken', accessToken);
          localStorage.setItem('refreshToken', newRefreshToken);
          
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        // Refresh failed, redirect to login
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/auth/login';
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

// Company API
export const companyApi = {
  getAll: (params?: { page?: number; pageSize?: number; search?: string; industry?: string }) =>
    api.get('/companies', { params }),
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
