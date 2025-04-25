import React, { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAppAuth } from '../App'; // Импортируем хук из App.tsx

interface PrivateRouteProps {
  children: ReactNode; // Используем ReactNode для большей гибкости
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({ children }) => {
  const { currentUser, isLoadingAuth } = useAppAuth();
  const location = useLocation();

  if (isLoadingAuth) {
    // Пока идет проверка, показываем заглушку или ничего не рендерим
    return <div>Проверка авторизации...</div>; // Или ваш Spinner
  }

  if (!currentUser) {
    // Если пользователя нет и проверка завершена, перенаправляем на логин
    // Сохраняем текущий путь, чтобы вернуться после входа
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Если пользователь есть, рендерим дочерний компонент
  return <>{children}</>; 
};

export default PrivateRoute; 