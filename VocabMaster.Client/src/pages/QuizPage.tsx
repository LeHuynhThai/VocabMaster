import React, { useState, useEffect } from 'react';
import useToast from '../hooks/useToast';
import quizService, { QuizQuestion, QuizResult, QuizStats } from '../services/quizService';
import { MESSAGES } from '../utils/constants';
import { useQuizStats } from '../contexts/QuizStatsContext';
import './QuizPage.css';

const QuizPage: React.FC = () => {
  const { showToast } = useToast();
  const { refreshStats } = useQuizStats(); // Use the quiz stats context
  const [question, setQuestion] = useState<QuizQuestion | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [selectedAnswer, setSelectedAnswer] = useState<string>('');
  const [result, setResult] = useState<QuizResult | null>(null);
  const [isChecking, setIsChecking] = useState<boolean>(false);
  const [shuffledOptions, setShuffledOptions] = useState<string[]>([]);
  const [stats, setStats] = useState<QuizStats | null>(null);
  const [loadingStats, setLoadingStats] = useState<boolean>(false);

  useEffect(() => {
    loadQuestion();
    loadStats();
  }, []);

  // Effect to shuffle the options when the question changes
  useEffect(() => {
    if (question) {
      const options = [
        question.wrongAnswer1,
        question.wrongAnswer2,
        question.wrongAnswer3,
        question.correctAnswer
      ];
      // Shuffle only once and save to state
      setShuffledOptions(options.sort(() => Math.random() - 0.5));
    }
  }, [question]);

  const loadQuestion = async () => {
    setLoading(true);
    setError('');
    setSelectedAnswer('');
    setResult(null);

    try {
      const questionData = await quizService.getRandomUncompletedQuestion();
      if (!questionData) {
        throw new Error('Không nhận được dữ liệu câu hỏi từ server');
      }
      setQuestion(questionData);
    } catch (error: any) {
      console.error('Error fetching quiz question:', error);
      // Hiển thị thông báo lỗi cụ thể hơn
      const errorMessage = error.response?.data?.message || MESSAGES.ERROR_QUIZ_FETCH;
      setError(errorMessage);
      showToast(errorMessage, 'danger');
    } finally {
      setLoading(false);
    }
  };

  const loadStats = async () => {
    setLoadingStats(true);
    try {
      const statsData = await quizService.getQuizStats();
      setStats(statsData);
    } catch (error) {
      console.error('Error fetching quiz stats:', error);
      // Don't show toast because this is just supplementary information
    } finally {
      setLoadingStats(false);
    }
  };

  const handleOptionClick = (option: string) => {
    if (result) return; // Prevent changing answer after result is shown
    setSelectedAnswer(option);
  };

  const handleCheckAnswer = async () => {
    if (!question || !selectedAnswer || isChecking) return;

    setIsChecking(true);

    try {
      // Use the new API to mark the question as completed
      const resultData = await quizService.checkAnswerAndComplete(question.id, selectedAnswer);
      setResult(resultData);
      
      // If the answer is correct, update stats and refresh the stats in sidebar
      if (resultData.isCorrect) {
        await loadStats(); // Update local stats
        refreshStats(); // Trigger refresh in the sidebar component
      }
    } catch (error) {
      console.error('Error checking answer:', error);
      showToast('Đã xảy ra lỗi khi kiểm tra câu trả lời', 'danger');
    } finally {
      setIsChecking(false);
    }
  };

  const handleNextQuestion = () => {
    loadQuestion();
  };

  const getOptionClassName = (option: string) => {
    if (!result) {
      return `quiz-option ${selectedAnswer === option ? 'selected' : ''}`;
    }

    if (option === result.correctAnswer) {
      return 'quiz-option correct';
    }

    if (selectedAnswer === option && !result.isCorrect) {
      return 'quiz-option incorrect';
    }

    return 'quiz-option';
  };

  const formatPercentage = (value: number): string => {
    return value.toFixed(1) + '%';
  };

  return (
    <div className="quiz-container">
      <div className="quiz-header">
        <h1 className="quiz-title">Trắc nghiệm từ vựng</h1>
        <p className="quiz-subtitle">Chọn nghĩa tiếng Việt chính xác của từ tiếng Anh</p>
      </div>

      {loading ? (
        <div className="quiz-loading">Đang tải câu hỏi...</div>
      ) : error ? (
        <div className="quiz-error">
          <p>{error}</p>
          <button className="btn btn-primary" onClick={loadQuestion}>Thử lại</button>
        </div>
      ) : question ? (
        <>
          <div className="quiz-question">
            <div className="quiz-word">{question.word}</div>

            <div className="quiz-options">
              {shuffledOptions.map((option, index) => (
                <div
                  key={index}
                  className={getOptionClassName(option)}
                  onClick={() => handleOptionClick(option)}
                >
                  {option}
                </div>
              ))}
            </div>

            {result && (
              <div className={`quiz-feedback ${result.isCorrect ? 'correct' : 'incorrect'}`}>
                {result.message}
                {!result.isCorrect && <div><strong>Đáp án đúng:</strong> {result.correctAnswer}</div>}
              </div>
            )}

            <div className="quiz-controls">
              {!result ? (
                <button 
                  className="btn btn-primary" 
                  onClick={handleCheckAnswer}
                  disabled={!selectedAnswer || isChecking}
                >
                  {isChecking ? 'Đang kiểm tra...' : 'Kiểm tra'}
                </button>
              ) : (
                <button 
                  className="btn btn-primary" 
                  onClick={handleNextQuestion}
                >
                  Câu hỏi tiếp theo
                </button>
              )}
            </div>
          </div>
        </>
      ) : (
        <div className="quiz-error">
          <p>Không có câu hỏi nào.</p>
          <button className="btn btn-primary" onClick={loadQuestion}>Thử lại</button>
        </div>
      )}
    </div>
  );
};

export default QuizPage; 