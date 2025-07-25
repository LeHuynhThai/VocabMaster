// User related types
export interface User {
  id: number;
  name: string;
  role: string;
  learnedWordsCount?: number;
}

export interface LoginRequest {
  name: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  password: string;
}

// Vocabulary related types
export interface Vocabulary {
  id: number;
  word: string;
  phonetic?: string;
  phonetics?: Phonetic[];
  meanings?: Meaning[];
}

export interface LearnedWord {
  id: number;
  word: string;
  userId: number;
}

// Dictionary response types
export interface DictionaryResponse {
  word: string;
  phonetic: string;
  phonetics: Phonetic[];
  meanings: Meaning[];
}

export interface Phonetic {
  text: string;
  audio: string;
}

export interface Meaning {
  partOfSpeech: string;
  definitions: Definition[];
}

export interface Definition {
  text: string;
  example: string;
  synonyms: string[];
  antonyms: string[];
} 