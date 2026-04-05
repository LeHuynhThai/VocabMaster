import React, { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuizStats } from '../contexts/QuizStatsContext';
import { useToast } from '../contexts/ToastContext';
import quizService, { QuizQuestion, QuizStats, SubmitAnswerResponse } from '../services/quizService';
import { logger } from '../utils/logger';
import { MESSAGES, ROUTES } from '../utils/constants';

type ToastVariant = 'success' | 'danger' | 'warning' | 'info';

const QuizPage: React.FC = () => {
  const { addToast } = useToast();
  const { refreshStats } = useQuizStats();
  const [question, setQuestion] = useState<QuizQuestion | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedAnswer, setSelectedAnswer] = useState('');
  const [result, setResult] = useState<SubmitAnswerResponse | null>(null);
  const [isChecking, setIsChecking] = useState(false);
  const [shuffledOptions, setShuffledOptions] = useState<string[]>([]);
  const [stats, setStats] = useState<QuizStats | null>(null);
  const [allCompleted, setAllCompleted] = useState(false);
  const [completionMessage, setCompletionMessage] = useState('');

  const showToast = useCallback((message: string, type: ToastVariant) => {
    const mappedType = type === 'danger' ? 'error' : type;

    addToast({
      message,
      type: mappedType,
    });
  }, [addToast]);

  const loadStats = useCallback(async () => {
    try {
      const statsData = await quizService.getQuizStats();
      setStats(statsData);
    } catch (loadStatsError) {
      logger.error('Error fetching quiz stats', loadStatsError);
    }
  }, []);

  const loadQuestion = useCallback(async () => {
    setLoading(true);
    setError('');
    setSelectedAnswer('');
    setResult(null);
    setAllCompleted(false);
    setCompletionMessage('');

    try {
      const response = await quizService.getRandomUncompletedQuestion();

      if ('allCompleted' in response && response.allCompleted) {
        setAllCompleted(true);
        setCompletionMessage(response.message);
        setQuestion(null);
        await loadStats();
      } else {
        setQuestion(response as QuizQuestion);
      }
    } catch (loadQuestionError: any) {
      logger.error('Error fetching quiz question', loadQuestionError);
      const errorMessage = loadQuestionError.response?.data?.message || MESSAGES.ERROR_QUIZ_FETCH;
      setError(errorMessage);
      showToast(errorMessage, 'danger');
    } finally {
      setLoading(false);
    }
  }, [loadStats, showToast]);

  useEffect(() => {
    void loadQuestion();
    void loadStats();
  }, [loadQuestion, loadStats]);

  useEffect(() => {
    if (!question) {
      setShuffledOptions([]);
      return;
    }

    const options = [
      question.wrongAnswer1,
      question.wrongAnswer2,
      question.wrongAnswer3,
      question.correctAnswer,
    ];

    setShuffledOptions(options.sort(() => Math.random() - 0.5));
  }, [question]);

  const handleOptionClick = (option: string) => {
    if (result) {
      return;
    }

    setSelectedAnswer(option);
  };

  const handleCheckAnswer = async () => {
    if (!question || !selectedAnswer || isChecking) {
      return;
    }

    setIsChecking(true);

    try {
      const resultData = await quizService.submitAnswer(question.id, selectedAnswer);
      setResult(resultData);
      await loadStats();
      refreshStats();
      showToast(resultData.message, resultData.isCorrect ? 'success' : 'danger');
    } catch (submitError) {
      logger.error('Error checking answer', submitError);
      showToast('Đã xảy ra lỗi khi kiểm tra câu trả lời', 'danger');
    } finally {
      setIsChecking(false);
    }
  };

  const getOptionClassName = (option: string) => {
    const baseClass = 'flex min-h-[70px] items-center justify-center rounded-2xl border-2 bg-white p-6 text-center text-lg transition-all duration-200';

    if (!result) {
      return [
        baseClass,
        selectedAnswer === option
          ? 'border-brand-primary bg-indigo-50 shadow-[0_0_0_2px_rgba(106,17,203,0.1)]'
          : 'border-slate-200 hover:-translate-y-0.5 hover:border-indigo-200 hover:bg-slate-50 hover:shadow-sm',
      ].join(' ');
    }

    if (option === question?.correctAnswer) {
      return `${baseClass} border-emerald-500 bg-emerald-50 shadow-[0_0_0_2px_rgba(82,196,26,0.1)]`;
    }

    if (selectedAnswer === option && !result.isCorrect) {
      return `${baseClass} border-rose-500 bg-rose-50 shadow-[0_0_0_2px_rgba(255,77,79,0.1)]`;
    }

    return `${baseClass} border-slate-200`;
  };

  const handleNextQuestion = () => {
    void loadQuestion();
  };

  const formatPercentage = (value: number): string => {
    return `${value.toFixed(1)}%`;
  };

  return (
    <div className="mx-auto flex min-h-[calc(100vh-240px)] max-w-4xl flex-col justify-center rounded-[1.5rem] bg-white px-6 py-10 shadow-[0_6px_16px_rgba(0,0,0,0.08)] sm:px-8">
      {loading ? (
        <div className="flex min-h-[300px] flex-col items-center justify-center gap-3 text-lg text-slate-500">
          <div className="h-10 w-10 animate-spin rounded-full border-4 border-slate-200 border-t-brand-primary"></div>
          <p className="m-0">Đang tải câu hỏi...</p>
        </div>
      ) : error ? (
        <div className="flex min-h-[250px] flex-col items-center justify-center gap-6 rounded-2xl border border-rose-200 bg-rose-50 px-6 py-10 text-center text-rose-700 shadow-sm">
          <p>{error}</p>
          <button
            type="button"
            className="rounded-xl bg-brand-gradient px-5 py-3 font-medium text-white shadow-sm transition hover:shadow-md"
            onClick={loadQuestion}
          >
            Thử lại
          </button>
        </div>
      ) : allCompleted ? (
        <div className="mx-auto flex w-full max-w-2xl flex-1 flex-col items-center justify-center text-center">
          <div className="mb-10 w-full border-b border-slate-200 pb-6 text-center">
            <h1 className="text-4xl font-semibold text-slate-800">Trắc nghiệm từ vựng</h1>
          </div>

          <div className="flex flex-col items-center justify-center px-2">
            <div className="mb-6 text-6xl text-amber-400">
              <i className="bi bi-trophy-fill"></i>
            </div>
            <h2 className="mb-6 text-3xl font-semibold text-emerald-600">{completionMessage}</h2>

            {stats ? (
              <div className="mb-8 w-full max-w-xl rounded-[1.25rem] border border-slate-200 bg-[linear-gradient(135deg,#f8f9fa_0%,#e9ecef_100%)] p-6 shadow-[0_4px_15px_rgba(0,0,0,0.1)]">
                <div className="space-y-3">
                  <div className="flex items-center justify-between border-b border-slate-300 py-3 last:border-b-0">
                    <span className="font-medium text-slate-600">Tổng số câu hỏi:</span>
                    <span className="bg-brand-gradient bg-clip-text text-lg font-bold text-transparent">{stats.totalQuestions}</span>
                  </div>
                  <div className="flex items-center justify-between border-b border-slate-300 py-3 last:border-b-0">
                    <span className="font-medium text-slate-600">Đã hoàn thành:</span>
                    <span className="bg-brand-gradient bg-clip-text text-lg font-bold text-transparent">{stats.completedQuestions}</span>
                  </div>
                  <div className="flex items-center justify-between border-b border-slate-300 py-3 last:border-b-0">
                    <span className="font-medium text-slate-600">Câu trả lời đúng:</span>
                    <span className="bg-brand-gradient bg-clip-text text-lg font-bold text-transparent">{stats.correctAnswers}</span>
                  </div>
                  <div className="flex items-center justify-between py-3">
                    <span className="font-medium text-slate-600">Tỷ lệ chính xác:</span>
                    <span className="bg-brand-gradient bg-clip-text text-lg font-bold text-transparent">{formatPercentage(stats.accuracyRate)}</span>
                  </div>
                </div>
              </div>
            ) : (
              <div className="mb-8 w-full max-w-xl rounded-[1.25rem] border border-slate-200 bg-slate-50 p-6 shadow-sm">
                <div className="flex items-center justify-between py-3">
                  <span className="font-medium text-slate-600">Đang tải thống kê...</span>
                </div>
              </div>
            )}

            <div className="mt-2">
              <Link
                to={ROUTES.QUIZ_STATS}
                className="inline-flex items-center rounded-xl bg-brand-gradient px-5 py-3 font-medium text-white no-underline shadow-sm transition hover:shadow-md"
              >
                <i className="bi bi-bar-chart-fill mr-2"></i>
                Xem thống kê chi tiết
              </Link>
            </div>
          </div>
        </div>
      ) : question ? (
        <div className="mx-auto flex w-full max-w-3xl flex-1 flex-col justify-center">
          <div className="mb-10 border-b border-slate-200 pb-6 text-center">
            <h1 className="text-4xl font-semibold text-slate-800">Trắc nghiệm từ vựng</h1>
          </div>

          <div className="w-full rounded-2xl bg-slate-50 p-6">
            <div className="mb-10 rounded-2xl bg-white p-8 text-center shadow-[0_4px_12px_rgba(0,0,0,0.06)]">
              <span className="inline-block border-b-[3px] border-brand-primary px-8 py-2 text-4xl font-medium tracking-[0.5px] text-slate-900 sm:text-5xl">
                {question.word}
              </span>
            </div>

            <div className="mb-8 grid gap-4 md:grid-cols-2">
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
              <div
                className={[
                  'mb-6 rounded-2xl border px-5 py-4 text-center text-lg shadow-sm',
                  result.isCorrect
                    ? 'border-emerald-200 bg-emerald-50 text-emerald-600'
                    : 'border-rose-200 bg-rose-50 text-rose-600',
                ].join(' ')}
              >
                <div>{result.message}</div>
                {!result.isCorrect && (
                  <div className="mt-2">
                    <strong>Đáp án đúng:</strong> {question.correctAnswer}
                  </div>
                )}
              </div>
            )}

            <div className="mt-6 flex justify-center">
              {!result ? (
                <button
                  type="button"
                  className="min-w-[180px] rounded-xl bg-brand-gradient px-6 py-3 text-lg font-medium text-white shadow-[0_4px_8px_rgba(0,0,0,0.1)] transition hover:-translate-y-0.5 hover:shadow-[0_6px_12px_rgba(0,0,0,0.15)] disabled:cursor-not-allowed disabled:opacity-70"
                  onClick={handleCheckAnswer}
                  disabled={!selectedAnswer || isChecking}
                >
                  {isChecking ? 'Đang kiểm tra...' : 'Kiểm tra'}
                </button>
              ) : (
                <button
                  type="button"
                  className="min-w-[180px] rounded-xl bg-brand-gradient px-6 py-3 text-lg font-medium text-white shadow-[0_4px_8px_rgba(0,0,0,0.1)] transition hover:-translate-y-0.5 hover:shadow-[0_6px_12px_rgba(0,0,0,0.15)]"
                  onClick={handleNextQuestion}
                >
                  Câu hỏi tiếp theo
                </button>
              )}
            </div>
          </div>
        </div>
      ) : (
        <div className="flex min-h-[250px] flex-col items-center justify-center gap-6 rounded-2xl border border-rose-200 bg-rose-50 px-6 py-10 text-center text-rose-700 shadow-sm">
          <p>Không có câu hỏi nào.</p>
          <button
            type="button"
            className="rounded-xl bg-brand-gradient px-5 py-3 font-medium text-white shadow-sm transition hover:shadow-md"
            onClick={loadQuestion}
          >
            Thử lại
          </button>
        </div>
      )}
    </div>
  );
};

export default QuizPage;