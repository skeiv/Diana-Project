import axios from 'axios';
import { User, Vacancy, Course, Notification, Resume, Message } from '../types';

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Добавляем перехватчик для добавления токена
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Добавляем перехватчик для обработки ошибок
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const userApi = {
  getProfile: (userId?: number) => 
    api.get<User>(`/users/${userId}`),
  
  updateProfile: (userId: number, data: Partial<User>) =>
    api.put(`/users/${userId}`, data),
  
  uploadAvatar: (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post<{ avatarUrl: string }>('/users/avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },
  
  deleteAvatar: () =>
    api.delete('/users/avatar'),

  register: (userData: Partial<User>) =>
    api.post<{ token: string; user: User }>('/auth/register', userData),

  login: (email: string, password: string) =>
    api.post<{ token: string; user: User }>('/auth/login', { email, password }),
};

export const vacancyApi = {
  getVacancies: (query?: string) =>
    api.get<Vacancy[]>(`/vacancies${query ? `?query=${query}` : ''}`),
  
  getManagedVacancies: () =>
    api.get<Vacancy[]>('/vacancies/managed-vacancies'),
  
  createVacancy: (data: Partial<Vacancy>) =>
    api.post<Vacancy>('/vacancies', data),
  
  updateVacancy: (id: number, data: Partial<Vacancy>) =>
    api.put(`/vacancies/${id}`, data),
  
  deleteVacancy: (id: number) =>
    api.delete(`/vacancies/${id}`),
  
  applyForVacancy: (vacancyId: number, resumeId: number) =>
    api.post(`/vacancies/${vacancyId}/apply`, { resumeId }),
  
  getApplications: () =>
    api.get('/vacancies/applications'),
  
  updateApplicationStatus: (applicationId: number, status: string) =>
    api.put(`/vacancies/applications/${applicationId}/status`, { status }),

  uploadImage: (vacancyId: number, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post<{ imageUrl: string }>(`/vacancies/${vacancyId}/image`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },
};

export const courseApi = {
  getCourses: (query?: string) =>
    api.get<Course[]>(`/courses${query ? `?query=${query}` : ''}`),
  
  getTeachingCourses: () =>
    api.get<Course[]>('/courses/teaching'),
  
  getEnrolledCourses: () =>
    api.get<Course[]>('/courses/enrolled'),
  
  createCourse: (data: Partial<Course>) =>
    api.post<Course>('/courses', data),
  
  updateCourse: (id: number, data: Partial<Course>) =>
    api.put(`/courses/${id}`, data),
  
  deleteCourse: (id: number) =>
    api.delete(`/courses/${id}`),
  
  enrollInCourse: (courseId: number) =>
    api.post(`/courses/${courseId}/enroll`),
  
  getCourseStatistics: (courseId: number) =>
    api.get(`/courses/${courseId}/statistics`),

  uploadImage: (courseId: number, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post<{ imageUrl: string }>(`/courses/${courseId}/image`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },
};

export const resumeApi = {
  createResume: (data: Partial<Resume>) =>
    api.post<Resume>('/resumes', data),
  
  updateResume: (id: number, data: Partial<Resume>) =>
    api.put(`/resumes/${id}`, data),
  
  deleteResume: (id: number) =>
    api.delete(`/resumes/${id}`),
  
  getResume: (userId: number) =>
    api.get<Resume>(`/resumes/user/${userId}`),
};

export const notificationApi = {
  getNotifications: () =>
    api.get<Notification[]>('/notifications'),
  
  markAsRead: (notificationId: number) =>
    api.put(`/notifications/${notificationId}/read`),
  
  markAllAsRead: () =>
    api.put('/notifications/read-all'),
};

export const messageApi = {
  getMessages: (userId: number) =>
    api.get<Message[]>(`/messages/${userId}`),
  
  sendMessage: (receiverId: number, content: string) =>
    api.post<Message>('/messages', { receiverId, content }),
  
  markAsRead: (messageId: number) =>
    api.put(`/messages/${messageId}/read`),
};

export default api; 