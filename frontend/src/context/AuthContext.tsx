import React, { createContext, useState, useEffect, useContext } from 'react';
import api from '../api/axios';
import { getToken, setToken, clearAuth } from './authStorage';

interface AuthContextValue {
  token: string | null;
  login: (token: string) => void;
  logout: () => void;
  authenticated: boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider: React.FC<{children: React.ReactNode}> = ({children}) => {
  const [token, setTok] = useState<string | null>(null);

  useEffect(() => {
    const t = getToken();
    if (t) setTok(t);
  }, []);

  useEffect(() => {
    // attach token to axios is handled in axios interceptor
  }, [token]);

  const login = (t: string) => { setTok(t); setToken(t); }
  const logout = () => { setTok(null); clearAuth(); window.location.href = '/login'; }

  return (
    <AuthContext.Provider value={{ token, login, logout, authenticated: !!token }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(){
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
