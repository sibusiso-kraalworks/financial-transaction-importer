'use client';

import React, { createContext, useContext, useState, useEffect } from 'react';
import { authService } from '@/services/api';
import toast from 'react-hot-toast';
import { User, AuthResult } from '@/types';

interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<boolean>;
  register: (email: string, password: string) => Promise<boolean>;
  logout: () => void;
  isLoading: boolean;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('token');
    const email = localStorage.getItem('userEmail');
    
    if (token && email) {
      setUser({ email, role: 'User' });
    }
    setIsLoading(false);
  }, []);

  const login = async (email: string, password: string): Promise<boolean> => {
    try {
      const result: AuthResult = await authService.login(email, password);
      
      if (result.success) {
        localStorage.setItem('token', result.token);
        localStorage.setItem('userEmail', result.email);
        setUser({ email: result.email, role: result.role });
        toast.success('Login successful!');
        return true;
      } else {
        toast.error(result.message || 'Login failed');
        return false;
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Login failed');
      return false;
    }
  };

  const register = async (email: string, password: string): Promise<boolean> => {
    try {
      const result: AuthResult = await authService.register(email, password);
      
      if (result.success) {
        localStorage.setItem('token', result.token);
        localStorage.setItem('userEmail', result.email);
        setUser({ email: result.email, role: result.role });
        toast.success('Registration successful!');
        return true;
      } else {
        toast.error(result.message || 'Registration failed');
        return false;
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Registration failed');
      return false;
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userEmail');
    setUser(null);
    toast.success('Logged out successfully');
  };

  return (
    <AuthContext.Provider value={{
      user,
      login,
      register,
      logout,
      isLoading,
      isAuthenticated: !!user,
    }}>
      {children}
    </AuthContext.Provider>
  );
};