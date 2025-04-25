import React from 'react';
import { useAppAuth } from '../App'; // Импортируем хук из App.tsx

const Dashboard: React.FC = () => {
  const { currentUser, logout } = useAppAuth();

  return (
    <div>
      <h2>Панель управления</h2>
      {currentUser ? (
        <div>
          <p>Привет, {currentUser.firstName || currentUser.email}!</p>
          <button onClick={logout}>Выйти</button>
        </div>
      ) : (
        <p>Пользователь не загружен.</p>
      )}
      {/* Здесь будет содержимое дашборда */}
    </div>
  );
};

export default Dashboard; 