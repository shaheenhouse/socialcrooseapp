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
};

// Order API
export const orderApi = {
  getAll: (params?: { page?: number; pageSize?: number; status?: string }) =>
    api.get('/orders', { params }),
  getById: (id: string) => api.get(`/orders/${id}`),
  create: (data: Record<string, unknown>) => api.post('/orders', data),
  cancel: (id: string, reason?: string) =>
    api.post(`/orders/${id}/cancel`, { reason }),
};
