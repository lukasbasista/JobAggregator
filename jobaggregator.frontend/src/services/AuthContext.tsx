import React, { createContext, useState, useEffect } from 'react';
import axios from 'axios';
import { jwtDecode } from 'jwt-decode';

interface User {
  id: string;
  username: string;
  roles: string[];
}

interface AuthContextProps {
  isAuthenticated: boolean;
  user: User | null;
  login: (token: string) => void;
  logout: () => void;
  isLoading: boolean;
}

interface AuthProviderProps {
  children: React.ReactNode;
}

interface DecodedToken {
  id: string;
  username: string;
  role: string | string[];
  exp: number;
}

export const AuthContext = createContext<AuthContextProps>(null!);

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const initializeAuth = () => {
      if (token) {
        try {
          const decoded: DecodedToken = jwtDecode(token);
          console.log('Decoded JWT Token:', decoded);

          const userData: User = {
            id: decoded.id,
            username: decoded.username,
            roles: Array.isArray(decoded.role) ? decoded.role : [decoded.role],
          };

          setUser(userData);
          axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
        } catch (error) {
          console.error('Token decoding failed', error);
          setUser(null);
        }
      } else {
        setUser(null);
        delete axios.defaults.headers.common['Authorization'];
      }
      setIsLoading(false);
    };

    initializeAuth();
  }, [token]);

  const login = (token: string) => {
    localStorage.setItem('token', token);
    setToken(token);
  };

  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated: !!user, user, login, logout, isLoading }}>
      {children}
    </AuthContext.Provider>
  );
};
