import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import quizService, { QuizStats } from '../../services/quizService';
import { useQuizStats } from '../../contexts/QuizStatsContext';
import { ROUTES } from '../../utils/constants';
import './QuizStats.css';

/**
 * Component for displaying user's quiz statistics
 * Shows total questions, completion rate, and correct answer rate
 * Clickable to navigate to detailed stats page
 */
const QuizStatsComponent: React.FC = () => {
  const [stats, setStats] = useState<QuizStats | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const { lastRefresh } = useQuizStats();

  useEffect(() => {
    // Fetch quiz statistics when component mounts or when lastRefresh changes
    const fetchStats = async () => {
      try {
        setLoading(true);
        const data = await quizService.getQuizStats();
        setStats(data);
        setError(null);
      } catch (err) {
        console.error('Error fetching quiz statistics:', err);
        setError('Không thể tải thống kê.');
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, [lastRefresh]); // Re-fetch when lastRefresh changes

  // Show loading state
  if (loading) {
    return <div className="quiz-stats-loading">Đang tải...</div>;
  }

  // Show error state
  if (error || !stats) {
    return <div className="quiz-stats-error">{error}</div>;
  }

  return (
    <Link to={ROUTES.QUIZ_STATS} className="quiz-stats-link">
      <div className="quiz-stats">
        <h3 className="quiz-stats-title">Thống kê trắc nghiệm</h3>
        
        <div className="quiz-stats-grid">
          {/* Total questions */}
          <div className="stat-item">
            <div className="stat-value">{stats.totalQuestions}</div>
            <div className="stat-label">Tổng câu</div>
          </div>
          
          {/* Completed questions */}
          <div className="stat-item">
            <div className="stat-value">{stats.completedQuestions}</div>
            <div className="stat-label">Đã hoàn thành</div>
          </div>
          
          {/* Correct answers */}
          <div className="stat-item">
            <div className="stat-value">{stats.correctAnswers}</div>
            <div className="stat-label">Câu đúng</div>
          </div>
        </div>

        {/* Progress bars */}
        <div className="quiz-progress-section">
          {/* Completion progress */}
          <div className="progress-item">
            <div className="progress-label">
              <span>Tiến độ hoàn thành</span>
              <span>{Math.round(stats.totalQuestions > 0 ? (stats.completedQuestions / stats.totalQuestions) * 100 : 0)}%</span>
            </div>
            <div className="progress-bar">
              <div 
                className="progress-fill" 
                style={{ width: `${stats.totalQuestions > 0 ? (stats.completedQuestions / stats.totalQuestions) * 100 : 0}%` }}
              ></div>
            </div>
          </div>
          
          {/* Accuracy progress */}
          <div className="progress-item">
            <div className="progress-label">
              <span>Tỷ lệ chính xác</span>
              <span>{Math.round(stats.accuracyRate)}%</span>
            </div>
            <div className="progress-bar">
              <div 
                className="progress-fill accuracy" 
                style={{ width: `${stats.accuracyRate}%` }}
              ></div>
            </div>
          </div>
        </div>
      </div>
    </Link>
  );
};

export default QuizStatsComponent; 