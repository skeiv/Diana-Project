import { ReactNode } from 'react';
import { User, UserRole, RegisterUserData } from '../types';
import api from '../services/api';

// Удаляем AuthContextType, AuthContext, AuthProvider, useAuth

// Если функции login/logout/register нужны отдельно, их нужно будет переписать
// Пример того, как они могли бы выглядеть (без управления состоянием):

// Функции для взаимодействия с API аутентификации

/**
 * Отправляет запрос на вход пользователя.
 * Сохраняет токен в localStorage при успехе.
 * @returns Данные пользователя и токен.
 * @throws Ошибка при неудачном входе.
 */
export const login = async (email: string, password: string): Promise<{ token: string; user: User }> => {
  try {
    const response = await api.post<{ token: string; user: User }>('/auth/login', {
      email,
      password,
    });
    localStorage.setItem('token', response.data.token);
    // Устанавливаем токен для последующих запросов api
    api.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`;
    return response.data;
  } catch (error) {
    console.error('Ошибка при входе:', error);
    // Очищаем токен на всякий случай
    localStorage.removeItem('token');
    delete api.defaults.headers.common['Authorization'];
    throw new Error('Не удалось войти. Проверьте email и пароль.'); // Более общее сообщение для пользователя
  }
};

/**
 * Выход пользователя. Удаляет токен из localStorage.
 */
export const logout = (): void => {
  localStorage.removeItem('token');
  // Удаляем токен из заголовков api
  delete api.defaults.headers.common['Authorization'];
  // Здесь не нужно возвращать значение или бросать ошибку (если только сама операция не может провалиться)
};

/**
 * Отправляет запрос на регистрацию пользователя.
 * Сохраняет токен в localStorage при успехе.
 * @returns Данные пользователя и токен.
 * @throws Ошибка при неудачной регистрации.
 */
export const register = async (userData: RegisterUserData): Promise<{ token: string; user: User }> => {
  try {
    const response = await api.post<{ token: string; user: User }>('/auth/register', userData);
    localStorage.setItem('token', response.data.token);
     // Устанавливаем токен для последующих запросов api
    api.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`;
    return response.data;
  } catch (error) {
    console.error('Ошибка при регистрации:', error);
     // Очищаем токен на всякий случай
    localStorage.removeItem('token');
    delete api.defaults.headers.common['Authorization'];
    throw new Error('Не удалось зарегистрироваться. Возможно, пользователь с таким email уже существует.'); // Более общее сообщение
  }
};

/**
 * Проверяет наличие токена и получает данные текущего пользователя.
 * Устанавливает заголовок Authorization для api.
 * @returns Данные пользователя или null, если токен невалиден или отсутствует.
 */
export const checkAuthStatus = async (): Promise<User | null> => {
    const token = localStorage.getItem('token');
    if (!token) {
        delete api.defaults.headers.common['Authorization'];
        return null;
    }

    api.defaults.headers.common['Authorization'] = `Bearer ${token}`;

    try {
        // Предполагаем, что есть эндпоинт для получения текущего пользователя по токену
        const response = await api.get<User>('/users/me');
        return response.data;
    } catch (error) {
        console.error('Ошибка при проверке статуса аутентификации:', error);
        localStorage.removeItem('token'); // Удаляем невалидный токен
        delete api.defaults.headers.common['Authorization'];
        return null;
    }
};

// Весь предыдущий код, связанный с AuthProvider/AuthContext/useAuth, удален. 