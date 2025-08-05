import React, { useEffect, useState } from 'react';
import quizService, { QuizStats, CompletedQuiz, PaginatedResponse } from '../services/quizService';
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
  const [totalPages, setTotalPages] = useState<number>(1);
  const pageSize = 10;

  // Fetch quiz stats
  const fetchStats = async () => {
    try {
      const statsData = await quizService.getQuizStats();
      setStats(statsData);
      return true;
    } catch (err) {
      console.error('Error fetching quiz statistics:', err);
      return false;
    }
  };

  // Fetch paginated correct quizzes
  const fetchPaginatedCorrectQuizzes = async (page: number = currentPage) => {
    try {
      // Luôn sử dụng pageSize = 10
      const response: PaginatedResponse<CompletedQuiz> = await quizService.getPaginatedCorrectQuizzes(page);
      setCorrectQuizzes(response.items);
      setTotalPages(response.pageInfo.totalPages);
      setCurrentPage(response.pageInfo.currentPage);
      return true;
    } catch (err) {
      console.error('Error fetching paginated correct quizzes:', err);
      return false;
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);

        // Fetch stats and paginated correct quizzes
        const statsSuccess = await fetchStats();
        const quizzesSuccess = await fetchPaginatedCorrectQuizzes(1); // Start with page 1
        
        if (!statsSuccess && !quizzesSuccess) {
          setError('Không thể tải thống kê. Vui lòng thử lại sau.');
        }
      } catch (err) {
        console.error('Error in fetchData:', err);
        setError('Không thể tải thống kê. Vui lòng thử lại sau.');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [lastRefresh]);

  // Handle page change
  const handlePageChange = (page: number) => {
    fetchPaginatedCorrectQuizzes(page);
    // Scroll to top when changing page
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

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
                  {correctQuizzes.map((quiz, index) => (
                    <tr key={quiz.id}>
                      <td>{(currentPage - 1) * pageSize + index + 1}</td>
                      <td>{quiz.quizQuestionId}</td>
                      <td className="stats-quiz-word">{quiz.word || 'N/A'}</td>
                      <td>{formatDate(quiz.completedAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            
            {/* Pagination component */}
            {totalPages > 1 && (
              <div className="quiz-stats-pagination">
                <Pagination
                  currentPage={currentPage}
                  totalPages={totalPages}
                  onPageChange={handlePageChange}
                />
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default QuizStatsPage; 