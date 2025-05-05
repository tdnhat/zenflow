/**
 * Role types available in the system
 */
export enum UserRole {
  User = 'USER',
  Admin = 'ADMIN',
  SuperAdmin = 'SUPER_ADMIN',
}

/**
 * Represents a user in the system
 */
export interface User {
  id: string;
  email: string;
  name: string;
  role: UserRole;
  avatar?: string;
  createdAt: string;
  updatedAt?: string;
}

/**
 * User credentials for login
 */
export interface UserCredentials {
  email: string;
  password: string;
}

/**
 * Authentication response containing tokens and user info
 */
export interface AuthResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}
