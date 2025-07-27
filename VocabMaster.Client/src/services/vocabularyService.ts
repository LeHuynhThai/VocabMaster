import api from './api';
import { Vocabulary, LearnedWord } from '../types';

/**
 * Service for vocabulary operations
 */
const vocabularyService = {
  /**
   * Gets a random word
   * @returns Random word data
   */
  getRandomWord: async (): Promise<Vocabulary> => {
    const response = await api.get('/api/wordgenerator/getrandomword');
    return response.data;
  },

  /**
   * Gets a new random word (forces refresh)
   * @returns Random word data
   */
  getNewRandomWord: async (): Promise<Vocabulary> => {
    const response = await api.get('/api/wordgenerator/getnewrandomword');
    return response.data;
  },

  /**
   * Looks up a word definition
   * @param word - Word to look up
   * @returns Word definition
   */
  lookup: async (word: string): Promise<Vocabulary> => {
    const response = await api.get(`/api/wordgenerator/lookup/${encodeURIComponent(word)}`);
    return response.data;
  },

  /**
   * Checks if a word is learned
   * @param word - Word to check
   * @returns Whether the word is learned
   */
  isLearned: async (word: string): Promise<boolean> => {
    const response = await api.get(`/api/wordgenerator/islearned/${encodeURIComponent(word)}`);
    return response.data.isLearned;
  },

  /**
   * Gets user's learned words
   * @param cacheBuster - Optional parameter to bypass cache
   * @returns List of learned words
   */
  getLearnedWords: async (cacheBuster?: string): Promise<LearnedWord[]> => {
    try {
      const url = cacheBuster ? `/api/learnedword${cacheBuster}` : '/api/learnedword';
      const response = await api.get(url);
      return response.data || [];
    } catch (error: any) {
      console.error('Error fetching learned words:', error);
      // return empty array if error
      return [];
    }
  },

  /**
   * Gets details of a learned word
   * @param id - ID of the learned word
   * @returns Learned word details
   */
  getLearnedWord: async (id: number): Promise<Vocabulary> => {
    try {
      const response = await api.get(`/api/learnedword/${id}`);
      return response.data;
    } catch (error: any) {
      console.error('Error fetching learned word details:', error);
      throw error;
    }
  },

  /**
   * Adds a word to learned list
   * @param word - Word to add
   * @returns Result of the operation
   */
  addLearnedWord: async (word: string): Promise<any> => {
    try {
      const response = await api.post(`/api/wordgenerator/learned/${encodeURIComponent(word)}`);
      return {
        success: true,
        data: response.data
      };
    } catch (error: any) {
      console.error('Error adding learned word:', error);
      // return error object instead of throwing exception
      return {
        success: false,
        error: error?.response?.data?.message || 'Không thể lưu từ vựng'
      };
    }
  },

  /**
   * Removes a word from learned list
   * @param id - ID of the learned word to remove
   * @returns Result of the operation
   */
  removeLearnedWord: async (id: number): Promise<any> => {
    try {
      const response = await api.delete(`/api/learnedword/${id}`);
      return {
        success: true,
        data: response.data
      };
    } catch (error: any) {
      console.error('Error removing learned word:', error);
      return {
        success: false,
        error: error?.response?.data?.message || 'Không thể xóa từ vựng'
      };
    }
  }
};

export default vocabularyService; 