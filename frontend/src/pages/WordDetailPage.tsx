import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Vocabulary } from '../types';
import api from '../services/api';
import VocabularyDetailCard from '../features/vocabulary/components/VocabularyDetailCard';

const WordDetailPage: React.FC = () => {
  const { word } = useParams<{ word: string }>();
  const navigate = useNavigate();
  const [vocabulary, setVocabulary] = useState<Vocabulary | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (word) {
      fetchWordDetail(word);
    }
  }, [word]);

  const fetchWordDetail = async (w: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await api.get(`/api/wordgenerator/word-detail/${encodeURIComponent(w)}`);
      setVocabulary(response.data);
    } catch (err) {
      setError('Không thể tải thông tin từ vựng.');
      console.error('Error fetching word detail:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const playAudio = (audioUrl: string) => {
    if (!audioUrl) return;
    try {
      const audio = new Audio(audioUrl);
      audio.play().catch(err => console.error('Error playing audio:', err));
    } catch (err) {
      console.error('Error creating audio element:', err);
    }
  };

  if (isLoading) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8">
        <div className="flex justify-center">
          <div className="h-10 w-10 animate-spin rounded-full border-4 border-slate-200 border-t-brand-primary"></div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8">
        <div className="rounded-2xl border border-rose-200 bg-rose-50 p-6 text-rose-700">
          <h2 className="mb-2 text-xl font-semibold">Lỗi</h2>
          <p>{error}</p>
          <button
            type="button"
            className="rounded-xl border border-rose-300 px-4 py-2 text-sm font-medium transition hover:bg-rose-100"
            onClick={() => navigate(-1)}
          >
            Quay lại
          </button>
        </div>
      </div>
    );
  }

  if (!vocabulary) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8">
        <div className="rounded-2xl border border-amber-200 bg-amber-50 p-6 text-amber-800">
          <h2 className="mb-2 text-xl font-semibold">Không tìm thấy</h2>
          <p>Không tìm thấy từ vựng "{word}"</p>
          <button
            type="button"
            className="rounded-xl border border-amber-300 px-4 py-2 text-sm font-medium transition hover:bg-amber-100"
            onClick={() => navigate(-1)}
          >
            Quay lại
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <div className="mb-4 flex items-center justify-between">
        <button
          type="button"
          className="inline-flex items-center rounded-xl border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
          onClick={() => navigate(-1)}
        >
          <i className="bi bi-arrow-left mr-2"></i>
          Quay lại
        </button>
      </div>

      <VocabularyDetailCard vocabulary={vocabulary} onPlayAudio={playAudio} />
    </div>
  );
};

export default WordDetailPage;

