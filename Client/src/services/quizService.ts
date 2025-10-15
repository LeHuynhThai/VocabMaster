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

// Interface for response when all questions are completed
export interface AllCompletedResponse {
  allCompleted: boolean;
  message: string;
  stats: QuizStats;
}

// Union type for quiz question response
export type QuizQuestionResponse = QuizQuestion | AllCompletedResponse;

export interface QuizResult {
  isCorrect: boolean;
  correctAnswer: string;
  message: string;
}

export interface SubmitAnswerResponse {
  isCorrect: boolean;
  message: string;
}

export interface CompletedQuiz {
  id: number;
  quizQuestionId: number;
  word: string;
  correctAnswer: string;
  completedAt: string;
  wasCorrect: boolean;
}

export interface QuizStats {
  totalQuestions: number;
  completedQuestions: number;
  correctAnswers: number;
  accuracyRate: number;
}

export interface PageInfo {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

// Interface cho kết quả phân trang
export interface PaginatedResponse<T> {
  items: T[];
  pageInfo: PageInfo;
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
   * @returns Random uncompleted quiz question or completion message
   */
  getRandomUncompletedQuestion: async (): Promise<QuizQuestionResponse> => {
    try {
      const response = await api.get(API_ENDPOINTS.QUIZ_RANDOM_UNCOMPLETED);
      
      // Check if response indicates all questions are completed
      if (response.data.completed) {
        return {
          allCompleted: true,
          message: response.data.message,
          stats: {
            totalQuestions: 0,
            completedQuestions: 0,
            correctAnswers: 0,
            accuracyRate: 0
          }
        } as AllCompletedResponse;
      }
      
      // Return the question data
      return response.data as QuizQuestion;
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
   * Gets paginated correctly answered quizzes
   * @param pageNumber - Page number (1-based)
   * @returns Paginated response with correctly answered quizzes
   */
  getPaginatedCorrectQuizzes: async (pageNumber: number = 1): Promise<PaginatedResponse<CompletedQuiz>> => {
    try {
      const response = await api.get(API_ENDPOINTS.QUIZ_CORRECT_PAGINATED, {
        params: { pageNumber }
      });
      return response.data;
    } catch (error: any) {
      console.error('Error fetching paginated correct quizzes:', error);
      // Return empty paginated response if error
      return {
        items: [],
        pageInfo: {
          currentPage: pageNumber,
          pageSize: 10, // default page size 10
          totalItems: 0,
          totalPages: 0
        }
      };
    }
  },

  /**
   * Submits a quiz answer
   * @param quizQuestionId ID of the quiz question
   * @param selectedAnswer Selected answer text
   * @returns Result of the answer submission
   */
  submitAnswer: async (quizQuestionId: number, selectedAnswer: string): Promise<SubmitAnswerResponse> => {
    try {
      const response = await api.post(API_ENDPOINTS.QUIZ_SUBMIT_ANSWER, {
        QuizQuestionId: quizQuestionId,
        SelectedAnswer: selectedAnswer
      });
      return response.data;
    } catch (error: any) {
      console.error('Error submitting quiz answer:', error);
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
  },

  /**
   * Gets completed answers list
   * @returns List of completed answers
   */
  getCompletedAnswers: async (): Promise<CompletedQuiz[]> => {
    try {
      console.log('Calling getCompletedAnswers with endpoint:', API_ENDPOINTS.QUIZ_COMPLETED_ANSWERS);
      const response = await api.get(API_ENDPOINTS.QUIZ_COMPLETED_ANSWERS);
      console.log('getCompletedAnswers response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('Error getting completed answers:', error);
      console.error('Error details:', error.response?.data);
      throw error;
    }
  }
};

export default quizService; 