import React, { useEffect, useState } from 'react';
import quizService, { QuizStats, CompletedQuiz } from '../services/quizService';
import { useQuizStats } from '../contexts/QuizStatsContext';
import '../components/ui/QuizStats.css';
import './QuizStatsPage.css';

/**
 * Quiz Statistics Page
 * Shows detailed statistics about user's quiz performance
 */
const QuizStatsPage: React.FC = () => {
  const [stats, setStats] = useState<QuizStats | null>(null);
  const [completedQuizzes, setCompletedQuizzes] = useState<CompletedQuiz[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const { lastRefresh } = useQuizStats();

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);

        // Create a default stats object in case the API call fails
        const defaultStats: QuizStats = {
          totalQuestions: 0,
          completedQuestions: 0,
          correctAnswers: 0,
          completionPercentage: 0,
          correctPercentage: 0
        };

        let statsData: QuizStats = defaultStats;
        let completedData: CompletedQuiz[] = [];

        try {
          // Try to get stats
          statsData = await quizService.getQuizStats();
        } catch (err) {
          console.error('Error fetching quiz statistics:', err);
        }

        try {
          // Try to get completed quizzes
          completedData = await quizService.getCompletedQuizzes();
        } catch (err) {
          console.error('Error fetching completed quizzes:', err);
        }
        
        setStats(statsData);
        setCompletedQuizzes(completedData);
      } catch (err) {
        console.error('Error in fetchData:', err);
        setError('Không thể tải thống kê. Vui lòng thử lại sau.');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [lastRefresh]);

  if (loading) {
    return (
      <div className="quiz-stats-page">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Đang tải thống kê...</p>
        </div>
      </div>
    );
  }

  // Even if there's an error, we'll try to display whatever data we have
  // Only show error page if we have absolutely no data
  if (error && !stats) {
    return (
      <div className="quiz-stats-page">
        <div className="error-container">
          <p>{error}</p>
        </div>
      </div>
    );
  }

  // Format date for display
  const formatDate = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch (err) {
      return dateString;
    }
  };

  // Make sure stats is defined
  const displayStats = stats || {
    totalQuestions: 0,
    completedQuestions: 0,
    correctAnswers: 0,
    completionPercentage: 0,
    correctPercentage: 0
  };

  return (
    <div className="quiz-stats-page">
      <div className="quiz-stats-header">
        <h1>Thống kê trắc nghiệm</h1>
        <p>Chi tiết về tiến độ và kết quả trắc nghiệm của bạn</p>
      </div>

      {/* Summary cards */}
      <div className="stats-summary">
        <div className="stats-card">
          <div className="stats-card-value">{displayStats.totalQuestions}</div>
          <div className="stats-card-label">Tổng số câu hỏi</div>
        </div>
        
        <div className="stats-card">
          <div className="stats-card-value">{displayStats.completedQuestions}</div>
          <div className="stats-card-label">Đã hoàn thành</div>
        </div>
        
        <div className="stats-card">
          <div className="stats-card-value">{displayStats.correctAnswers}</div>
          <div className="stats-card-label">Trả lời đúng</div>
        </div>
        
        <div className="stats-card">
          <div className="stats-card-value">{Math.round(displayStats.correctPercentage)}%</div>
          <div className="stats-card-label">Tỷ lệ chính xác</div>
        </div>
      </div>

      {/* Progress section */}
      <div className="stats-progress-section">
        <h2>Tiến độ hoàn thành</h2>
        <div className="stats-progress-container">
          <div className="stats-progress-bar">
            <div 
              className="stats-progress-fill" 
              style={{ width: `${displayStats.completionPercentage}%` }}
            ></div>
          </div>
          <div className="stats-progress-text">
            {Math.round(displayStats.completionPercentage)}% ({displayStats.completedQuestions}/{displayStats.totalQuestions})
          </div>
        </div>
      </div>

      {/* Recent completed quizzes */}
      <div className="completed-quizzes-section">
        <h2>Lịch sử làm bài gần đây</h2>
        {completedQuizzes.length === 0 ? (
          <p className="no-data-message">Bạn chưa hoàn thành câu hỏi nào.</p>
        ) : (
          <div className="completed-quizzes-table-container">
            <table className="completed-quizzes-table">
              <thead>
                <tr>
                  <th>ID câu hỏi</th>
                  <th>Thời gian</th>
                  <th>Kết quả</th>
                </tr>
              </thead>
              <tbody>
                {completedQuizzes.slice(0, 10).map((quiz) => (
                  <tr key={quiz.id}>
                    <td>{quiz.quizQuestionId}</td>
                    <td>{formatDate(quiz.completedAt)}</td>
                    <td>
                      <span className={quiz.wasCorrect ? 'correct-answer' : 'wrong-answer'}>
                        {quiz.wasCorrect ? 'Đúng' : 'Sai'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default QuizStatsPage; 