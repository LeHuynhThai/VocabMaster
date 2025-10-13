import api from './api';
import { Vocabulary } from '../types';
import { API_ENDPOINTS } from '../utils/constants';

// Interface for learned word
export interface LearnedWord {
  id: number;
  word: string;
  learnedAt?: string;
}

// Interface for pagination info
export interface PageInfo {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

// Interface for paginated response
export interface PaginatedResponse<T> {
  items: T[];
  pageInfo: PageInfo;
}

/**
 * Service for vocabulary operations
 */
const vocabularyService = {
  /**
   * Gets a random word
   * @returns Random word data
   */
  getRandomWord: async (): Promise<Vocabulary> => {
    const response = await api.get(API_ENDPOINTS.RANDOM_WORD);
    return response.data;
  },

  /**
   * Gets a new random word (forces refresh)
   * @returns Random word data
   */
  getNewRandomWord: async (): Promise<Vocabulary> => {
    const response = await api.get(API_ENDPOINTS.RANDOM_WORD);
    return response.data;
  },


  /**
   * Gets user's learned words
   * @param cacheBuster - Optional parameter to bypass cache
   * @returns List of learned words
   */
  getLearnedWords: async (cacheBuster?: string): Promise<LearnedWord[]> => {
    try {
      const url = cacheBuster ? `${API_ENDPOINTS.LEARNED_WORDS}?t=${cacheBuster}` : API_ENDPOINTS.LEARNED_WORDS;
      const response = await api.get(url);
      return response.data || [];
    } catch (error: any) {
      // Gracefully handle missing endpoint or auth issues by returning an empty list
      // to keep the UI functional without spamming console errors.
      console.warn('Learned words endpoint unavailable, returning empty list.');
      return [];
    }
  },
  


  /**
   * Marks a word as learned
   * @param word - Word to mark as learned
   * @returns Success status
   */
  markAsLearned: async (word: string): Promise<boolean> => {
    try {
      const response = await api.post(API_ENDPOINTS.ADD_LEARNED_WORD, { word });
      return response.data && response.data.success;
    } catch (error: any) {
      console.error('Error marking word as learned:', error);
      return false;
    }
  },

  /**
   * Removes a learned word
   * @param id - ID of the learned word to remove
   * @returns Success status
   */
  removeLearnedWord: async (id: number): Promise<boolean> => {
    try {
      await api.delete(`${API_ENDPOINTS.LEARNED_WORDS}/${id}`);
      return true;
    } catch (error: any) {
      console.error('Error removing learned word:', error);
      return false;
    }
  }
};

export default vocabularyService; 