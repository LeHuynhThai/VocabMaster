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
   * Marks a word as learned
   * @param word - Word to mark as learned
   * @returns Success status
   */
  markAsLearned: async (word: string): Promise<boolean> => {
    try {
      const response = await api.post(`/api/wordgenerator/learned/${encodeURIComponent(word)}`);
      return response.data.success;
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
      const response = await api.delete(`/api/learnedword/${id}`);
      return true;
    } catch (error: any) {
      console.error('Error removing learned word:', error);
      return false;
    }
  }
};

export default vocabularyService; 