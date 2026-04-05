import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useToast } from '../contexts/ToastContext';
import VocabularyDetailCard from '../features/vocabulary/components/VocabularyDetailCard';
import vocabularyService from '../services/vocabularyService';
import { logger } from '../utils/logger';
import { Vocabulary } from '../types';
import { ROUTES } from '../utils/constants';

const WordGeneratorPage: React.FC = () => {
  const [word, setWord] = useState<Vocabulary | null>(null);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const { addToast } = useToast();
  const location = useLocation();
  const navigate = useNavigate();
  const [allLearned, setAllLearned] = useState(false);
  const [allLearnedMessage, setAllLearnedMessage] = useState('');

  const getWordFromUrl = useCallback(() => {
    const queryParams = new URLSearchParams(location.search);
    return queryParams.get('word');
  }, [location.search]);

  const fetchRandomWord = useCallback(async () => {
    setLoading(true);
    try {
      const data = await vocabularyService.getRandomWord();

      if (data.allLearned) {
        setAllLearned(true);
        setAllLearnedMessage(data.message || 'Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống.');
        setWord(null);
      } else {
        setWord(data);
        setAllLearned(false);
        setAllLearnedMessage('');
      }

      navigate(ROUTES.WORD_GENERATOR, { replace: true });
    } catch (error) {
      logger.error('Error fetching random word', error);
      addToast({
        type: 'error',
        message: 'Bạn đã học hết tất cả từ vựng trong hệ thống.',
      });
    } finally {
      setLoading(false);
    }
  }, [addToast, navigate]);

  const fetchNewRandomWord = useCallback(async () => {
    setLoading(true);
    try {
      const data = await vocabularyService.getNewRandomWord();

      if (data.allLearned) {
        setAllLearned(true);
        setAllLearnedMessage(data.message || 'Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống.');
        setWord(null);
      } else {
        setWord(data);
        setAllLearned(false);
        setAllLearnedMessage('');
      }

      navigate(ROUTES.WORD_GENERATOR, { replace: true });
    } catch (error) {
      logger.error('Error fetching new random word', error);
      addToast({
        type: 'error',
        message: 'Bạn đã học hết tất cả từ vựng trong hệ thống.',
      });
    } finally {
      setLoading(false);
    }
  }, [addToast, navigate]);

  const handleSaveWord = async (e?: React.MouseEvent) => {
    if (e) {
      e.preventDefault();
    }
    if (!word || word.isLearned || saving) {
      return;
    }

    setSaving(true);
    try {
      const success = await vocabularyService.markAsLearned(word.word);

      if (success) {
        setWord({ ...word, isLearned: true });
        addToast({
          type: 'success',
          message: `Đã lưu từ "${word.word}" vào danh sách từ vựng đã học.`,
        });
      } else {
        addToast({
          type: 'error',
          message: 'Không thể lưu từ vựng. Vui lòng thử lại sau.',
        });
      }
    } catch (error) {
      logger.error('Error saving word', error);
      addToast({
        type: 'error',
        message: 'Không thể lưu từ vựng. Vui lòng thử lại sau.',
      });
    } finally {
      setSaving(false);
    }
  };

  const playAudio = (audioUrl: string) => {
    if (!audioUrl) {
      return;
    }

    try {
      const audio = new Audio(audioUrl);
      audio.play().catch((error) => {
        logger.error('Error playing audio', error);
      });
    } catch (error) {
      logger.error('Error creating audio element', error);
    }
  };

  useEffect(() => {
    const wordFromUrl = getWordFromUrl();

    if (wordFromUrl) {
      addToast({
        type: 'info',
        message: 'Chức năng tìm kiếm từ URL tạm thời không khả dụng',
      });
      navigate(ROUTES.WORD_GENERATOR, { replace: true });
      fetchRandomWord();
    } else {
      fetchRandomWord();
    }
  }, [getWordFromUrl, fetchRandomWord, addToast, navigate]);

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      {loading ? (
        <div className="my-10 flex flex-col items-center justify-center gap-3 text-slate-500">
          <div className="h-10 w-10 animate-spin rounded-full border-4 border-slate-200 border-t-brand-primary"></div>
          <p className="m-0">Đang tải từ vựng...</p>
        </div>
      ) : allLearned ? (
        <div className="mx-auto my-8 flex max-w-2xl flex-col items-center rounded-[1.75rem] bg-slate-50 p-8 text-center shadow-[0_4px_12px_rgba(0,0,0,0.1)]">
          <i className="bi bi-trophy-fill mb-3 text-5xl text-amber-400"></i>
          <h3 className="mb-4 text-2xl font-semibold text-emerald-600">{allLearnedMessage}</h3>
          <p className="mb-2 text-slate-600">Bạn đã hoàn thành việc học tất cả từ vựng có trong hệ thống.</p>
          <p className="mb-0 text-slate-600">Hãy tiếp tục ôn tập các từ đã học để củng cố kiến thức.</p>
          <button
            type="button"
            className="mt-4 inline-flex items-center rounded-xl bg-brand-gradient px-4 py-3 font-medium text-white shadow-sm transition hover:shadow-md"
            onClick={() => navigate(ROUTES.LEARNED_WORDS)}
          >
            <i className="bi bi-journal-text mr-2"></i>
            Xem danh sách từ đã học
          </button>
        </div>
      ) : word ? (
        <VocabularyDetailCard
          vocabulary={word}
          onPlayAudio={playAudio}
          actions={
            <>
              <button
                type="button"
                className="inline-flex items-center rounded-xl bg-brand-gradient px-4 py-3 font-medium text-white shadow-sm transition hover:shadow-md disabled:opacity-70"
                onClick={fetchNewRandomWord}
                disabled={loading}
              >
                <i className="bi bi-shuffle mr-1"></i>
                Từ vựng mới
              </button>

              <button
                type="button"
                className={[
                  'inline-flex items-center rounded-xl px-4 py-3 font-medium shadow-sm transition',
                  word.isLearned
                    ? 'cursor-not-allowed bg-emerald-600 text-white opacity-80'
                    : 'border border-emerald-600 bg-white text-emerald-700 hover:bg-emerald-50',
                ].join(' ')}
                onClick={handleSaveWord}
                disabled={word.isLearned || loading || saving}
              >
                {saving ? (
                  <span className="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent"></span>
                ) : (
                  <i className={`bi ${word.isLearned ? 'bi-check-circle-fill' : 'bi-plus-circle'} mr-1`}></i>
                )}
                {word.isLearned ? 'Đã lưu' : saving ? 'Đang lưu...' : 'Lưu từ này'}
              </button>
            </>
          }
        />
      ) : (
        <div className="my-10 text-center text-slate-600">
          <p>Không tìm thấy từ vựng. Vui lòng thử lại.</p>
          <button
            type="button"
            className="inline-flex items-center rounded-xl bg-brand-gradient px-4 py-3 font-medium text-white shadow-sm transition hover:shadow-md"
            onClick={fetchRandomWord}
          >
            Tải từ vựng ngẫu nhiên
          </button>
        </div>
      )}
    </div>
  );
};

export default WordGeneratorPage;