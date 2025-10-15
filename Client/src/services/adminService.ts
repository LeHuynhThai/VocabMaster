import api from './api';
import { API_ENDPOINTS } from '../utils/constants';

export interface AddVocabularyRequest {
  word: string;
  vietnamese: string;
  meaningsJson?: string;
  pronunciationsJson?: string;
}

export interface VocabularyResponse {
  id: number;
  word: string;
  vietnamese: string;
  meaningsJson: string;
  pronunciationsJson: string;
}

export interface ApiResponse<T> {
  message: string;
  data?: T;
}

const adminService = {
  /**
   * Add new vocabulary
   * @param request - Vocabulary data
   * @returns Added vocabulary
   */
  addVocabulary: async (request: AddVocabularyRequest): Promise<VocabularyResponse> => {
    try {
      const response = await api.post(API_ENDPOINTS.ADMIN_ADD_VOCABULARY, request);
      return response.data.data;
    } catch (error: any) {
      console.error('Error adding vocabulary:', error);
      throw error;
    }
  },

  /**
   * Delete vocabulary
   * @param vocabularyId - ID of vocabulary to delete
   * @returns Success status
   */
  deleteVocabulary: async (vocabularyId: number): Promise<boolean> => {
    try {
      const response = await api.delete(`${API_ENDPOINTS.ADMIN_DELETE_VOCABULARY}/${vocabularyId}`);
      return response.status === 200;
    } catch (error: any) {
      console.error('Error deleting vocabulary:', error);
      throw error;
    }
  }
};

export default adminService;
