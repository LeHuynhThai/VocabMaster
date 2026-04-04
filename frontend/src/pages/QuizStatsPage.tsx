import React, { useCallback, useState, useEffect } from 'react';
import quizService, { QuizStats, CompletedQuiz } from '../services/quizService';
import useToast from '../hooks/useToast';
import Pagination from '../components/ui/Pagination';

const QuizStatsPage: React.FC = () => {
  const [stats, setStats] = useState<QuizStats | null>(null);
  const [allCompletedAnswers, setAllCompletedAnswers] = useState<CompletedQuiz[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const { showToast } = useToast();

  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');
    
    try {
      // Load stats first
      const statsData = await quizService.getQuizStats();
      setStats(statsData);
      
       // Then load completed answers (separate try-catch to avoid blocking stats)
       try {
         const completedAnswersData = await quizService.getCompletedAnswers();
         setAllCompletedAnswers(completedAnswersData);
       } catch (completedAnswersError: any) {
         console.error('Error loading completed answers:', completedAnswersError);
         // Don't show error toast for completed answers, just log it
         setAllCompletedAnswers([]);
       }
    } catch (err: any) {
      console.error('Error loading quiz stats:', err);
      const errorMessage = err.response?.data?.message || 'Không thể tải thống kê. Vui lòng thử lại sau.';
      setError(errorMessage);
      showToast(errorMessage, 'danger');
    } finally {
      setLoading(false);
    }
  }, [showToast]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const formatPercentage = (value: number): string => {
    return value.toFixed(1) + '%';
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getCompletionPercentage = (): number => {
    if (!stats || stats.totalQuestions === 0) return 0;
    return (stats.completedQuestions / stats.totalQuestions) * 100;
  };

  // Pagination logic
  const totalPages = Math.ceil(allCompletedAnswers.length / pageSize);
  const startIndex = (currentPage - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const currentPageAnswers = allCompletedAnswers.slice(startIndex, endIndex);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  if (loading) {
    return (
      <section className="mx-auto max-w-6xl px-4 py-8 lg:ml-[250px]">
        <div className="flex flex-col items-center justify-center gap-3 py-10 text-slate-500">
          <div className="h-10 w-10 animate-spin rounded-full border-4 border-slate-200 border-t-brand-primary"></div>
          <span className="sr-only">Đang tải...</span>
          <p>Đang tải thống kê...</p>
        </div>
      </section>
    );
  }

  if (error) {
    return (
      <section className="mx-auto max-w-6xl px-4 py-8 lg:ml-[250px]">
        <div className="rounded-2xl border border-rose-200 bg-rose-50 p-6 text-rose-700">
          <h2 className="text-xl font-semibold">Lỗi</h2>
          <p>{error}</p>
          <button
            type="button"
            className="inline-flex items-center justify-center rounded-xl border border-rose-300 px-4 py-2 text-sm font-medium transition hover:bg-rose-100"
            onClick={loadData}
          >
            Thử lại
          </button>
        </div>
      </section>
    );
  }

  return (
    <section className="min-h-[calc(100vh-80px)] max-w-6xl bg-transparent px-4 py-8 lg:ml-[250px] lg:border-l lg:border-slate-200 lg:bg-[#f8f9fa]">
      <div className="mb-4">
        <h1 className="mb-2 text-3xl font-semibold text-slate-800">Thống kê trắc nghiệm</h1>
        <p className="text-slate-500">Theo dõi tiến độ học tập của bạn</p>
      </div>

      <div className="mb-4 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <div className="rounded-2xl bg-white p-6 text-center shadow-[0_2px_10px_rgba(0,0,0,0.1)] transition duration-200 hover:-translate-y-0.5 hover:shadow-[0_4px_20px_rgba(0,0,0,0.15)]">
              <div className="mb-2 text-4xl font-bold text-[#6f42c1]">{stats?.totalQuestions || 0}</div>
              <div className="text-sm font-medium text-slate-500">Tổng số câu hỏi</div>
        </div>
        <div className="rounded-2xl bg-white p-6 text-center shadow-[0_2px_10px_rgba(0,0,0,0.1)] transition duration-200 hover:-translate-y-0.5 hover:shadow-[0_4px_20px_rgba(0,0,0,0.15)]">
              <div className="mb-2 text-4xl font-bold text-[#6f42c1]">{stats?.completedQuestions || 0}</div>
              <div className="text-sm font-medium text-slate-500">Đã hoàn thành</div>
        </div>
        <div className="rounded-2xl bg-white p-6 text-center shadow-[0_2px_10px_rgba(0,0,0,0.1)] transition duration-200 hover:-translate-y-0.5 hover:shadow-[0_4px_20px_rgba(0,0,0,0.15)]">
              <div className="mb-2 text-4xl font-bold text-[#6f42c1]">{stats?.correctAnswers || 0}</div>
              <div className="text-sm font-medium text-slate-500">Trả lời đúng</div>
        </div>
        <div className="rounded-2xl bg-white p-6 text-center shadow-[0_2px_10px_rgba(0,0,0,0.1)] transition duration-200 hover:-translate-y-0.5 hover:shadow-[0_4px_20px_rgba(0,0,0,0.15)]">
              <div className="mb-2 text-4xl font-bold text-[#6f42c1]">{formatPercentage(stats?.accuracyRate || 0)}</div>
              <div className="text-sm font-medium text-slate-500">Tỷ lệ chính xác</div>
        </div>
      </div>

      <div className="mb-4 rounded-2xl bg-white p-6 shadow-[0_2px_10px_rgba(0,0,0,0.1)]">
        <div className="mb-2 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <h2 className="text-lg font-semibold text-slate-900">Tiến độ hoàn thành</h2>
          <span className="text-sm text-slate-500">
              {formatPercentage(getCompletionPercentage())} ({stats?.completedQuestions || 0}/{stats?.totalQuestions || 0})
          </span>
        </div>
        <div className="h-3 overflow-hidden rounded-full bg-slate-200">
          <div
            className="h-full rounded-full bg-brand-gradient transition-all duration-300"
            style={{ width: `${getCompletionPercentage()}%` }}
          ></div>
        </div>
      </div>

      <div className="rounded-2xl bg-white p-6 shadow-[0_2px_10px_rgba(0,0,0,0.1)]">
        <div className="mb-3 flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
            <div>
            <h2 className="text-lg font-semibold text-slate-900">Danh sách các câu đã hoàn thành</h2>
              {allCompletedAnswers.length > 0 && (
              <p className="mt-1 text-sm text-slate-500">
                  Hiển thị {startIndex + 1}-{Math.min(endIndex, allCompletedAnswers.length)} trong tổng số {allCompletedAnswers.length} câu
              </p>
              )}
            </div>
          <button
            type="button"
            className="inline-flex items-center justify-center rounded-xl border border-brand-primary px-4 py-2.5 text-sm font-medium text-brand-primary transition hover:bg-brand-primary/5 disabled:cursor-not-allowed disabled:opacity-60"
              onClick={loadData}
              disabled={loading}
            >
            <i className="bi bi-arrow-clockwise mr-1"></i>
              Làm mới
          </button>
          </div>

          {allCompletedAnswers.length === 0 ? (
          <div className="py-8 text-center">
              <i className="bi bi-inbox mb-3 block text-6xl text-slate-400"></i>
            <p className="text-slate-500">Bạn chưa hoàn thành câu hỏi nào.</p>
            </div>
          ) : (
            <>
            <div className="divide-y divide-slate-200">
                {currentPageAnswers.map((answer, index) => (
                <div key={answer.id} className="flex flex-col gap-3 px-0 py-4 transition hover:bg-slate-50 sm:flex-row sm:items-start sm:justify-between">
                  <div className="min-w-0 flex-1">
                    <div className="mb-1 flex items-center gap-2">
                      <span
                        className={[
                          'inline-flex rounded-full px-2.5 py-1 text-xs font-semibold',
                          answer.wasCorrect ? 'bg-emerald-100 text-emerald-800' : 'bg-rose-100 text-rose-800',
                        ].join(' ')}
                      >
                            #{startIndex + index + 1}
                          </span>
                          <strong className="text-[1.1rem] text-slate-800">{answer.word}</strong>
                        </div>
                    <div className={`mt-1 font-medium ${answer.wasCorrect ? 'text-emerald-600' : 'text-rose-600'}`}>
                      <i className={`bi mr-1 ${answer.wasCorrect ? 'bi-check-circle-fill' : 'bi-x-circle-fill'}`}></i>
                          {answer.correctAnswer}
                        </div>
                      </div>
                  <div className="text-sm text-slate-500">
                        {formatDate(answer.completedAt)}
                      </div>
                </div>
                ))}
            </div>

              {totalPages > 1 && (
            <div className="mt-4 flex justify-center">
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
    </section>
  );
};

export default QuizStatsPage;