import api from './api';
import { Vocabulary, LearnedWord, DictionaryResponse } from '../types';

const vocabularyService = {
  getRandomWord: async (): Promise<Vocabulary> => {
    const response = await api.get('/api/wordgenerator/getrandomword');
    return response.data;
  },

  getLearnedWords: async (): Promise<LearnedWord[]> => {
    const response = await api.get('/api/learnedword');
    return response.data;
  },

  addLearnedWord: async (word: string): Promise<LearnedWord> => {
    const response = await api.post('/api/learnedword', { word });
    return response.data;
  },

  removeLearnedWord: async (id: number): Promise<boolean> => {
    const response = await api.delete(`/api/learnedword/${id}`);
    return response.status === 200;
  },

  lookupWord: async (word: string): Promise<DictionaryResponse> => {
    const response = await api.get(`/api/wordgenerator/lookup/${word}`);
    return response.data;
  }
};

export default vocabularyService; 