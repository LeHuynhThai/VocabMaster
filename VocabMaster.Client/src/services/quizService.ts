import api from './api';
import { API_ENDPOINTS } from '../utils/constants';

export interface QuizQuestion {
  id: number;
  word: string;
  correctAnswer: string; // Đảm bảo correctAnswer không phải là optional
  wrongAnswer1: string;
  wrongAnswer2: string;
  wrongAnswer3: string;
}

export interface QuizResult {
  isCorrect: boolean;
  correctAnswer: string;
  message: string;
}

/**
 * Service for quiz operations
 */
const quizService = {
  /**
   * Gets a random quiz question
   * @returns Random quiz question
   */
  getRandomQuestion: async (): Promise<QuizQuestion> => {
    try {
      const response = await api.get(API_ENDPOINTS.QUIZ_RANDOM);
      return response.data;
    } catch (error: any) {
      console.error('Error getting random quiz question:', error);
      throw error;
    }
  },

  /**
   * Creates a new quiz question
   * @returns ID of the created question
   */
  createQuestion: async (): Promise<number> => {
    try {
      const response = await api.post(API_ENDPOINTS.QUIZ_CREATE);
      return response.data.questionId;
    } catch (error: any) {
      console.error('Error creating quiz question:', error);
      throw error;
    }
  },

  /**
   * Checks an answer to a quiz question
   * @param questionId ID of the question
   * @param selectedAnswer Selected answer text
   * @returns Result of the answer check
   */
  checkAnswer: async (questionId: number, selectedAnswer: string): Promise<QuizResult> => {
    try {
      const response = await api.post(API_ENDPOINTS.QUIZ_CHECK_ANSWER, {
        questionId,
        selectedAnswer
      });
      return response.data;
    } catch (error: any) {
      console.error('Error checking quiz answer:', error);
      throw error;
    }
  }
};

export default quizService; 