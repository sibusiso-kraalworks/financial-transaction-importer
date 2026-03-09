import axios from 'axios';
import { Transaction, PagedResult, TransactionUploadResult, UpdateTransactionDto } from '@/types';

const api = axios.create({
    //add baseURL from environment variable with fallback to localhost

  baseURL: (process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001') + '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('userEmail');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authService = {
  register: async (email: string, password: string) => {
    const response = await api.post('/auth/register', { Email: email, Password: password });
    return response.data;
  },
  
  login: async (email: string, password: string) => {
    const response = await api.post('/auth/login', { Email: email, Password: password });
    return response.data;
  },
};

export const transactionService = {
  upload: async (file: File): Promise<TransactionUploadResult> => {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await api.post('/transactions/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
  
  getAll: async (pageNumber: number = 1, pageSize: number = 10): Promise<PagedResult<Transaction>> => {
    const response = await api.get(`/transactions?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    return response.data;
  },
  
  getById: async (id: number): Promise<Transaction> => {
    const response = await api.get(`/transactions/${id}`);
    return response.data;
  },
  
  update: async (id: number, data: UpdateTransactionDto): Promise<Transaction> => {
    const response = await api.put(`/transactions/${id}`, data);
    return response.data;
  },
  
  delete: async (id: number): Promise<void> => {
    await api.delete(`/transactions/${id}`);
  },
};

export default api;