/**
 * Represents a user in the ZenFlow system
 */
export interface User {
  id: string;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  displayName?: string;
  roles: string[];
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
  lastLogin?: Date;
}

/**
 * Represents user profile information
 */
export interface UserProfile {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  displayName?: string;
  avatarUrl?: string;
  roles: string[];
  preferences?: UserPreferences;
}

/**
 * User-specific preferences for the application
 */
export interface UserPreferences {
  theme?: 'light' | 'dark' | 'system';
  language?: string;
  notifications?: {
    email: boolean;
    inApp: boolean;
  };
  editorSettings?: {
    snapToGrid: boolean;
    gridSize: number;
    showMinimap: boolean;
  };
}