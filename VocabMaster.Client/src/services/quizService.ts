import api from './api';
import { API_ENDPOINTS } from '../utils/constants';

export interface QuizQuestion {
  id: number;
  word: string;
  correctAnswer: string;
  wrongAnswer1: string;
  wrongAnswer2: string;
  wrongAnswer3: string;
}

export interface QuizResult {
  isCorrect: boolean;
  correctAnswer: string;
  message: string;
}

export interface CompletedQuiz {
  id: number;
  quizQuestionId: number;
  word: string;
  completedAt: string;
  wasCorrect: boolean;
}

export interface QuizStats {
  totalQuestions: number;
  completedQuestions: number;
  correctAnswers: number;
  completionPercentage: number;
  correctPercentage: number;
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
   * Gets a random uncompleted quiz question
   * @returns Random uncompleted quiz question
   */
  getRandomUncompletedQuestion: async (): Promise<QuizQuestion> => {
    try {
      const response = await api.get(API_ENDPOINTS.QUIZ_RANDOM_UNCOMPLETED);
      return response.data;
    } catch (error: any) {
      console.error('Error getting random uncompleted quiz question:', error);
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
  },

  /**
   * Checks an answer to a quiz question and marks it as completed
   * @param questionId ID of the question
   * @param selectedAnswer Selected answer text
   * @returns Result of the answer check
   */
  checkAnswerAndComplete: async (questionId: number, selectedAnswer: string): Promise<QuizResult> => {
    try {
      console.log('Calling checkAnswerAndComplete with:', { 
        questionId, 
        selectedAnswer, 
        endpoint: API_ENDPOINTS.QUIZ_CHECK_ANSWER_COMPLETE 
      });
      
      const response = await api.post(API_ENDPOINTS.QUIZ_CHECK_ANSWER_COMPLETE, {
        questionId,
        selectedAnswer
      });
      
      console.log('Response from checkAnswerAndComplete:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('Error checking and completing quiz answer:', error);
      throw error;
    }
  },

  /**
   * Gets all completed quizzes
   * @returns List of completed quizzes
   */
  getCompletedQuizzes: async (): Promise<CompletedQuiz[]> => {
    try {
      const response = await api.get(API_ENDPOINTS.QUIZ_COMPLETED);
      return response.data;
    } catch (error: any) {
      console.error('Error getting completed quizzes:', error);
      throw error;
    }
  },

  /**
   * Gets all correctly answered quizzes
   * @returns List of correctly answered quizzes
   */
  getCompleteQuizz: async (): Promise<CompletedQuiz[]> => {
    try {
      const response = await api.get(API_ENDPOINTS.QUIZ_CORRECT);
      return response.data;
    } catch (error: any) {
      console.error('Error getting correct quizzes:', error);
      throw error;
    }
  },

  /**
   * Gets quiz statistics
   * @returns Quiz statistics
   */
  getQuizStats: async (): Promise<QuizStats> => {
    try {
      const response = await api.get(API_ENDPOINTS.QUIZ_STATS);
      return response.data;
    } catch (error: any) {
      console.error('Error getting quiz stats:', error);
      throw error;
    }
  }
};

export default quizService; 