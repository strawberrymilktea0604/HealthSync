import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';

interface User {
  userId: number;
  email: string;
  fullName: string;
  role: string;
  token: string;
  expiresAt: Date;
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
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Check for stored token on app start
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      try {
        const parsedUser = JSON.parse(storedUser);
        if (new Date(parsedUser.expiresAt) > new Date()) {
          setUser(parsedUser);
        } else {
          localStorage.removeItem('user');
        }
      } catch (e) {
        localStorage.removeItem('user');
      }
    }
  }, []);

  const login = async (email: string, password: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await fetch('http://localhost:5274/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.Error || 'Login failed');
      }

      const data = await response.json();
      console.log("Backend response data:", data);
      
      const userData: User = {
        userId: data.UserId || data.userId,
        email: data.Email || data.email,
        fullName: data.FullName || data.fullName,
        role: data.Role || data.role,
        token: data.Token || data.token,
        expiresAt: new Date(data.ExpiresAt || data.expiresAt),
      };

      console.log("Mapped userData:", userData);
      
      setUser(userData);
      localStorage.setItem('user', JSON.stringify(userData));
      return userData;
    } catch (err) {
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
      const response = await fetch('http://localhost:5274/api/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.Error || 'Registration failed');
      }

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

  const value: AuthContextType = {
    user,
    login,
    register,
    logout,
    setUser: setUserData,
    isLoading,
    error,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};