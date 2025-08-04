/**
 * User information
 */
export interface User {
  id: number;
  name: string;
  role: string;
  learnedWordsCount?: number;
}

/**
 * Pronunciation information
 */
export interface Pronunciation {
  text: string;
  audio: string;
}

/**
 * Definition information
 */
export interface Definition {
  text: string;
  example: string;
  synonyms: string[];
  antonyms: string[];
}

/**
 * Meaning information
 */
export interface Meaning {
  partOfSpeech: string;
  definitions: Definition[];
}

/**
 * Vocabulary information
 */
export interface Vocabulary {
  id: number;
  word: string;
  phonetic: string;
  pronunciations: Pronunciation[];
  meanings: Meaning[];
  isLearned: boolean;
  vietnamese?: string;
  phoneticsJson?: string;
  meaningsJson?: string;
}

/**
 * Learned word information
 */
export interface LearnedWord {
  id: number;
  word: string;
  learnedDate?: string;
}

/**
 * Login request information
 */
export interface LoginRequest {
  name: string;
  password: string;
}

/**
 * Register request information
 */
export interface RegisterRequest {
  name: string;
  password: string;
  confirmPassword: string;
}

export interface TokenResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  userId: number;
  userName: string;
  role: string;
}

/**
 * Google Auth information
 */
export interface GoogleAuthRequest {
  accessToken: string;
  idToken?: string;
}

/**
 * Toast notification information
 */
export interface Toast {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  message: string;
  duration?: number;
  isExiting?: boolean;
} 