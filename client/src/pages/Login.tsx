import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAppAuth } from '../App'; // Импортируем хук из App.tsx

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { login, authError, isLoadingAuth } = useAppAuth(); // Получаем функцию login и состояние из контекста
  const navigate = useNavigate();

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    try {
      await login(email, password);
      navigate('/dashboard'); // Перенаправляем на дашборд после успешного входа
    } catch (error) {
      // Ошибка уже обработана в App.tsx и выведена в authError
      console.error("Login failed:", error);
    }
  };

  return (
    <div>
      <h2>Вход</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="email">Email:</label>
          <input
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="password">Пароль:</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        {authError && <p style={{ color: 'red' }}>{authError}</p>}
        <button type="submit" disabled={isLoadingAuth}>
          {isLoadingAuth ? 'Вход...' : 'Войти'}
        </button>
      </form>
      <p>
        Нет аккаунта? <Link to="/register">Зарегистрироваться</Link>
      </p>
    </div>
  );
};

export default Login; 