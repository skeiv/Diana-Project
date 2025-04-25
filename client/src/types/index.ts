export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  city: string;
  role: UserRole;
  avatarUrl?: string;
  isDeleted: boolean;
  resume?: Resume;
}

export enum UserRole {
  JobSeeker = 'JobSeeker',
  Employer = 'Employer',
  Teacher = 'Teacher',
  Recruiter = 'Recruiter',
  Admin = 'Admin'
}

export interface Vacancy {
  id: number;
  title: string;
  company: string;
  location: string;
  description: string;
  requirements: string[];
  salaryFrom?: number;
  salaryTo?: number;
  status: VacancyStatus;
  employerId: number;
  isDeleted: boolean;
  imageUrl?: string;
  applications: VacancyApplication[];
}

export enum VacancyStatus {
  Active = 'Active',
  Archived = 'Archived'
}

export interface Course {
  id: number;
  title: string;
  description: string;
  topics: string[];
  duration: number;
  teacherId: number;
  teacherName: string;
  isDeleted: boolean;
  imageUrl?: string;
  enrolledUsers: CourseEnrollment[];
}

export interface Resume {
  id: number;
  userId: number;
  title: string;
  experience: string;
  education: string;
  skills: string[];
  isDeleted: boolean;
}

export interface VacancyApplication {
  id: number;
  vacancyId: number;
  userId: number;
  resumeId: number;
  status: ApplicationStatus;
  createdAt: string;
  user: User;
  resume: Resume;
}

export enum ApplicationStatus {
  Pending = 'Pending',
  Accepted = 'Accepted',
  Rejected = 'Rejected'
}

export interface CourseEnrollment {
  id: number;
  courseId: number;
  userId: number;
  status: EnrollmentStatus;
  createdAt: string;
  user: User;
}

export enum EnrollmentStatus {
  Pending = 'Pending',
  Accepted = 'Accepted',
  Rejected = 'Rejected'
}

export interface Notification {
  id: number;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
  userId: number;
}

export enum NotificationType {
  NewVacancy = 'NewVacancy',
  NewCourse = 'NewCourse',
  ApplicationStatus = 'ApplicationStatus',
  CourseEnrolled = 'CourseEnrolled',
  NewMessage = 'NewMessage'
}

export interface Message {
  id: number;
  senderId: number;
  receiverId: number;
  content: string;
  createdAt: string;
  isRead: boolean;
  sender: User;
  receiver: User;
}

// Интерфейс для данных, отправляемых при регистрации
export interface RegisterUserData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone: string;
  city: string; // Добавим город, если он нужен при регистрации
  role: UserRole;
  // Можно добавить другие поля, если они нужны при регистрации
} 