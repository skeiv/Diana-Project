import React, { useState, useEffect, createContext, useContext, ReactNode, useCallback } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { login as apiLogin, logout as apiLogout, register as apiRegister, checkAuthStatus } from './hooks/useAuth';
import { User, RegisterUserData } from './types';
import Home from './pages/Home';
import Register from './pages/Register';
import Login from './pages/Login';
import PrivateRoute from './components/PrivateRoute';
import Dashboard from './pages/Dashboard';

interface AppAuthContextType {
  currentUser: User | null;
  isLoadingAuth: boolean;
  authError: string | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  register: (userData: RegisterUserData) => Promise<void>;
}

const AppAuthContext = createContext<AppAuthContextType | undefined>(undefined);

export const useAppAuth = () => {
  const context = useContext(AppAuthContext);
  if (context === undefined) {
    throw new Error('useAppAuth must be used within the App component');
  }
  return context;
};

const App: React.FC = () => {
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [isLoadingAuth, setIsLoadingAuth] = useState(true);
  const [authError, setAuthError] = useState<string | null>(null);

  useEffect(() => {
    const verifyAuth = async () => {
      setIsLoadingAuth(true);
      setAuthError(null);
      try {
        const user = await checkAuthStatus();
        setCurrentUser(user);
      } catch (err) {
        setCurrentUser(null);
      } finally {
        setIsLoadingAuth(false);
      }
    };
    verifyAuth();
  }, []);

  const handleLogin = useCallback(async (email: string, password: string) => {
    setIsLoadingAuth(true);
    setAuthError(null);
    try {
      const { user } = await apiLogin(email, password);
      setCurrentUser(user);
    } catch (error: any) {
      setCurrentUser(null);
      setAuthError(error.message || 'Ошибка входа');
      throw error;
    } finally {
      setIsLoadingAuth(false);
    }
  }, []);

  const handleLogout = useCallback(() => {
    setIsLoadingAuth(true);
    setAuthError(null);
    apiLogout();
    setCurrentUser(null);
    setIsLoadingAuth(false);
  }, []);

  const handleRegister = useCallback(async (userData: RegisterUserData) => {
    setIsLoadingAuth(true);
    setAuthError(null);
    try {
      const { user } = await apiRegister(userData);
      setCurrentUser(user);
    } catch (error: any) {
      setCurrentUser(null);
      setAuthError(error.message || 'Ошибка регистрации');
      throw error;
    } finally {
      setIsLoadingAuth(false);
    }
  }, []);

  const authContextValue: AppAuthContextType = {
    currentUser,
    isLoadingAuth,
    authError,
    login: handleLogin,
    logout: handleLogout,
    register: handleRegister,
  };

  if (isLoadingAuth && !currentUser) {
    return <div>Загрузка аутентификации...</div>;
  }

  return (
    <AppAuthContext.Provider value={authContextValue}>
      <Router>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/register" element={<Register />} />
          <Route path="/login" element={<Login />} />
          <Route
            path="/dashboard"
            element={
              <PrivateRoute>
                <Dashboard />
              </PrivateRoute>
            }
          />
        </Routes>
      </Router>
    </AppAuthContext.Provider>
  );
};

export default App; 