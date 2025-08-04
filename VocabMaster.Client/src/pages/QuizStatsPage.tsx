import React, { useEffect, useState } from 'react';
import quizService, { QuizStats, CompletedQuiz } from '../services/quizService';
import { useQuizStats } from '../contexts/QuizStatsContext';
import useToast from '../hooks/useToast';
import '../components/ui/QuizStats.css';
import './QuizStatsPage.css';
import Pagination from '../components/ui/Pagination';

/**
 * Quiz Statistics Page
 * Shows detailed statistics about user's quiz performance
 */
const QuizStatsPage: React.FC = () => {
  const [stats, setStats] = useState<QuizStats | null>(null);
  const [correctQuizzes, setCorrectQuizzes] = useState<CompletedQuiz[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const { lastRefresh } = useQuizStats();
  const { showToast } = useToast();
  
  // Pagination state
  const [currentPage, setCurrentPage] = useState<number>(1);
  const pageSize = 10;

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
        let correctData: CompletedQuiz[] = [];

        try {
          // Try to get stats
          statsData = await quizService.getQuizStats();
        } catch (err) {
          console.error('Error fetching quiz statistics:', err);
        }
        
        try {
          // Try to get correct quizzes
          correctData = await quizService.getCompleteQuizz();
        } catch (err) {
          console.error('Error fetching correct quizzes:', err);
        }
        
        setStats(statsData);
        setCorrectQuizzes(correctData);
        
        // Reset to first page when data changes
        setCurrentPage(1);
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

  // Calculate total pages and data for current page        
  const totalPages = Math.ceil(correctQuizzes.length / pageSize);
  const startIndex = (currentPage - 1) * pageSize;
  const endIndex = Math.min(startIndex + pageSize, correctQuizzes.length);
  const currentPageData = correctQuizzes.slice(startIndex, endIndex);

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  return (
    <div className="quiz-stats-page">
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

      {/* Correct quizzes section */}
      <div className="completed-quizzes-section">
        <h2>Danh sách các câu trả lời đúng</h2>
        {correctQuizzes.length === 0 ? (
          <p className="no-data-message">Bạn chưa trả lời đúng câu hỏi nào.</p>
        ) : (
          <>
            <div className="completed-quizzes-table-container">
              <table className="completed-quizzes-table">
                <thead>
                  <tr>
                    <th>STT</th>
                    <th>ID câu hỏi</th>
                    <th>Từ vựng</th>
                    <th>Thời gian</th>
                  </tr>
                </thead>
                <tbody>
                  {currentPageData.map((quiz, index) => (
                    <tr key={quiz.id}>
                      <td>{startIndex + index + 1}</td>
                      <td>{quiz.quizQuestionId}</td>
                      <td className="stats-quiz-word">{quiz.word || 'N/A'}</td>
                      <td>{formatDate(quiz.completedAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            
            {/* Pagination component */}
            <div className="quiz-stats-pagination">
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={handlePageChange}
              />
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default QuizStatsPage; 