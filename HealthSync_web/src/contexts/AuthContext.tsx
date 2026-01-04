import React, { createContext, useContext, useState, useEffect, ReactNode, useMemo } from 'react';
import authService, { AuthResponse } from '../services/authService';

interface User {
  userId: number;
  email: string;
  fullName: string;
  role: string;
  token: string;
  expiresAt: Date;
  isProfileComplete: boolean;
  avatar?: string; // Avatar URL from backend
}

interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<User>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => void;
  setUser: (user: User) => void;
  isLoading: boolean;
  error: string | null;
}

interface RegisterData {
  email: string;
  password: string;
  fullName: string;
  dateOfBirth: string;
  gender: string;
  heightCm: number;
  weightKg: number;
  verificationCode: string;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

/* eslint-disable react-refresh/only-export-components */
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true); // Start with true to check stored token
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Check for stored token on app start
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      try {
        const parsedUser = JSON.parse(storedUser);
        // Check if token is still valid
        const expiresAt = new Date(parsedUser.expiresAt);
        const now = new Date();

        if (expiresAt > now) {
          setUser(parsedUser);
        } else {
          // Token expired, clear storage
          localStorage.removeItem('user');
        }
      } catch (error) {
        console.error('Error parsing stored user:', error);
        localStorage.removeItem('user');
      }
    }
    setIsLoading(false); // Finished checking
  }, []);

  const login = async (email: string, password: string) => {
    // Check mạng trước khi gọi API
    if (!navigator.onLine) {
      throw new Error('Không có kết nối Internet. Vui lòng kiểm tra lại đường truyền.');
    }

    setIsLoading(true);
    setError(null);
    try {
      const data: AuthResponse = await authService.login({ email, password });
      console.log("Backend response data:", data);

      const userData: User = {
        userId: data.userId,
        email: data.email,
        fullName: data.fullName,
        role: data.role,
        token: data.token,
        expiresAt: new Date(data.expiresAt),
        isProfileComplete: data.role === 'Admin' ? true : data.isProfileComplete, // Admin không cần complete profile
        avatar: data.avatarUrl,
      };

      console.log("Mapped userData:", userData);

      setUser(userData);
      localStorage.setItem('user', JSON.stringify(userData));
      return userData;
    } catch (err) {
      // Check if it's a network error
      if (!navigator.onLine || (err instanceof TypeError && err.message.includes('fetch'))) {
        setError('Không thể kết nối đến server. Vui lòng thử lại sau.');
        throw new Error('Không thể kết nối đến server. Vui lòng thử lại sau.');
      }
      setError(err instanceof Error ? err.message : 'An error occurred');
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (data: RegisterData) => {
    setIsLoading(true);
    setError(null);
    try {
      await authService.register({
        email: data.email,
        password: data.password,
        fullName: data.fullName,
      });

      // Registration successful, but user needs to verify email
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('user');
  };

  const setUserData = (userData: User) => {
    setUser(userData);
    localStorage.setItem('user', JSON.stringify(userData));
  };

  const value = useMemo(() => ({
    user,
    login,
    register,
    logout,
    setUser: setUserData,
    isLoading,
    error,
  }), [user, isLoading, error]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};