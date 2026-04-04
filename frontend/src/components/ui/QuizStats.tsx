import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import quizService, { QuizStats } from '../../services/quizService';
import { useQuizStats } from '../../contexts/QuizStatsContext';
import { ROUTES } from '../../utils/constants';

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

  const completionRate = Math.round(
    stats && stats.totalQuestions > 0
      ? (stats.completedQuestions / stats.totalQuestions) * 100
      : 0
  );

  // Show loading state
  if (loading) {
    return <div className="rounded-lg bg-white px-4 py-4 text-center text-sm text-slate-500 shadow-sm">Đang tải...</div>;
  }

  // Show error state
  if (error || !stats) {
    return <div className="rounded-lg bg-rose-50 px-4 py-4 text-center text-sm text-rose-700 shadow-sm">{error}</div>;
  }

  return (
    <Link to={ROUTES.QUIZ_STATS} className="block text-inherit no-underline transition hover:-translate-y-0.5">
      <div className="rounded-lg bg-white p-4 shadow-sm transition hover:bg-slate-50">
        <h3 className="mb-4 border-b border-slate-200 pb-2 text-sm font-semibold text-slate-700">Thống kê trắc nghiệm</h3>

        <div className="mb-4 grid grid-cols-3 gap-3">
          <div className="text-center">
            <div className="text-xl font-bold text-[#6a11cb]">{stats.totalQuestions}</div>
            <div className="mt-1 text-xs text-slate-500">Tổng câu</div>
          </div>

          <div className="text-center">
            <div className="text-xl font-bold text-[#6a11cb]">{stats.completedQuestions}</div>
            <div className="mt-1 text-xs text-slate-500">Đã hoàn thành</div>
          </div>

          <div className="text-center">
            <div className="text-xl font-bold text-[#6a11cb]">{stats.correctAnswers}</div>
            <div className="mt-1 text-xs text-slate-500">Câu đúng</div>
          </div>
        </div>

        <div className="mt-4 space-y-3">
          <div>
            <div className="mb-1 flex justify-between text-xs text-slate-700">
              <span>Tiến độ hoàn thành</span>
              <span>{completionRate}%</span>
            </div>
            <div className="h-1.5 overflow-hidden rounded-full bg-slate-200">
              <div
                className="h-full rounded-full bg-[linear-gradient(90deg,#6a11cb_0%,#2575fc_100%)] transition-all duration-300"
                style={{ width: `${completionRate}%` }}
              ></div>
            </div>
          </div>

          <div>
            <div className="mb-1 flex justify-between text-xs text-slate-700">
              <span>Tỷ lệ chính xác</span>
              <span>{Math.round(stats.accuracyRate)}%</span>
            </div>
            <div className="h-1.5 overflow-hidden rounded-full bg-slate-200">
              <div
                className="h-full rounded-full bg-[linear-gradient(90deg,#11998e_0%,#38ef7d_100%)] transition-all duration-300"
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