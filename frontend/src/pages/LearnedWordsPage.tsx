import React, { useCallback, useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import vocabularyService, { LearnedWord } from '../services/vocabularyService';
import { logger } from '../utils/logger';
import { useAuth } from '../contexts/AuthContext';
import Pagination from '../components/ui/Pagination';
import useToast from '../hooks/useToast';

/**
 * Format date to local format
 */
const formatDate = (dateString?: string): string => {
  if (!dateString) return '';
  
  try {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    }).format(date);
  } catch (error) {
    return '';
  }
};

/**
 * LearnedWords page component
 * Displays all words that the user has saved with pagination
 */
const LearnedWordsPage: React.FC = () => {
  const [words, setWords] = useState<LearnedWord[]>([]);
  const [filteredWords, setFilteredWords] = useState<LearnedWord[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10); // Fixed page size of 10
  const [totalPages, setTotalPages] = useState(1);
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const { showToast } = useToast();

  /**
   * Fetch paginated learned words for the current user
   */
  const fetchLearnedWords = useCallback(async (page: number) => {
    if (!isAuthenticated) {
      return; // do not call API if not authenticated
    }
    
    setIsLoading(true);
    setError(null);
    
    try {
      // Use non-paginated endpoint and compute pagination client-side
      const all = await vocabularyService.getLearnedWords();
      const totalItemsLocal = all.length;
      const totalPagesLocal = Math.max(1, Math.ceil(totalItemsLocal / pageSize));
      const start = (page - 1) * pageSize;
      const items = all.slice(start, start + pageSize);

      setWords(items);
      setFilteredWords(items);
      setTotalPages(totalPagesLocal);
      setCurrentPage(page);
    } catch (err) {
      setError('Không thể tải danh sách các từ đã học. Vui lòng thử lại sau.');
      logger.error('Error fetching learned words', err);
    } finally {
      setIsLoading(false);
    }
  }, [isAuthenticated, pageSize]);

  /**
   * Remove a word from the learned words list
   */
  const removeWord = async (id: number, word: string) => {
    // Show confirmation dialog
    if (!window.confirm(`Bạn có chắc chắn muốn xóa từ "${word}" khỏi danh sách đã học?`)) {
      return;
    }

    setIsLoading(true);
    
    try {
      const success = await vocabularyService.removeLearnedWord(id);
      if (success) {
        showToast('Đã xóa từ vựng thành công', 'success');
        // Reload the current page to reflect changes
        fetchLearnedWords(currentPage);
      } else {
        showToast('Không thể xóa từ này. Vui lòng thử lại sau.', 'danger');
      }
    } catch (err) {
      showToast('Không thể xóa từ này. Vui lòng thử lại sau.', 'danger');
      logger.error('Error removing word', err);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handle search input change
   */
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const query = e.target.value;
    setSearchQuery(query);
    
    if (!query.trim()) {
      setFilteredWords(words);
      setTotalPages(Math.max(1, Math.ceil(words.length / pageSize)));
    } else {
      const filtered = words.filter(word => 
        word.word.toLowerCase().includes(query.toLowerCase())
      );
      setFilteredWords(filtered);
      setTotalPages(Math.max(1, Math.ceil(filtered.length / pageSize)));
    }
    
    // Reset to page 1 when searching
    setCurrentPage(1);
  };

  /**
   * Handle page change
   */
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    
    // If searching, paginate filtered results
    if (searchQuery.trim()) {
      const start = (page - 1) * pageSize;
      const end = start + pageSize;
      const paginatedFiltered = words
        .filter(word => word.word.toLowerCase().includes(searchQuery.toLowerCase()))
        .slice(start, end);
      setFilteredWords(paginatedFiltered);
    } else {
      // If not searching, fetch from server
      fetchLearnedWords(page);
    }
    
    // Scroll to top when changing page
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const viewWordDetail = (word: string) => {
    navigate(`/word-detail/${encodeURIComponent(word)}`);
  };

  // load learned words when component mounts or authentication changes
  useEffect(() => {
    if (isAuthenticated) {
      fetchLearnedWords(1);
    }
  }, [fetchLearnedWords, isAuthenticated]);

  // Reload data when page becomes visible
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible' && isAuthenticated) {
        fetchLearnedWords(currentPage);
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [currentPage, fetchLearnedWords, isAuthenticated]);

  // retry loading if there was an error
  const handleRetry = () => {
    fetchLearnedWords(currentPage);
  };

  const actionButtonClass =
    'inline-flex h-9 w-9 items-center justify-center rounded-lg border transition disabled:cursor-not-allowed disabled:opacity-60';

  return (
    <section className="mx-auto max-w-5xl px-4 py-8">
      <div className="mb-3 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <button
          type="button"
          onClick={() => fetchLearnedWords(currentPage)}
          disabled={isLoading}
          className="inline-flex items-center rounded-xl border border-brand-primary px-4 py-2.5 text-sm font-medium text-brand-primary transition hover:bg-brand-primary/5 disabled:cursor-not-allowed disabled:opacity-60"
        >
          <i className="bi bi-arrow-clockwise mr-2"></i>
          Làm mới
        </button>

        <div className="w-full md:max-w-md">
          <div className="flex overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm focus-within:border-brand-primary focus-within:ring-4 focus-within:ring-brand-primary/10">
            <input
              type="text"
              placeholder="Tìm kiếm từ vựng đã học..."
              value={searchQuery}
              onChange={handleSearchChange}
              disabled={isLoading}
              className="w-full bg-transparent px-4 py-3 text-slate-900 outline-none placeholder:text-slate-400"
            />
            <div className="flex items-center border-l border-slate-200 px-4 text-slate-400">
              <i className="bi bi-search"></i>
            </div>
          </div>
        </div>
      </div>

      {error && (
        <div className="mb-4 flex flex-col gap-3 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-4 text-rose-700 sm:flex-row sm:items-center sm:justify-between">
          <div>{error}</div>
          <button
            type="button"
            onClick={handleRetry}
            className="inline-flex items-center justify-center rounded-xl border border-rose-300 px-4 py-2 text-sm font-medium transition hover:bg-rose-100"
          >
            Thử lại
          </button>
        </div>
      )}

      <div className="overflow-hidden rounded-2xl bg-white shadow-[0_2px_10px_rgba(0,0,0,0.1)]">
        <div className="p-6">
          {isLoading ? (
            <div className="flex justify-center py-10">
              <div className="flex flex-col items-center gap-3 text-slate-500">
                <div className="h-10 w-10 animate-spin rounded-full border-4 border-slate-200 border-t-brand-primary"></div>
                <span className="sr-only">Đang tải...</span>
              </div>
            </div>
          ) : filteredWords.length === 0 ? (
            <div className="py-10 text-center">
              {searchQuery.trim() ? (
                <p className="mb-4 text-slate-500">Không tìm thấy từ vựng nào phù hợp với "{searchQuery}".</p>
              ) : (
                <p className="mb-4 text-slate-500">Bạn chưa lưu từ vựng nào.</p>
              )}
            </div>
          ) : (
            <>
              <div className="overflow-x-auto rounded-2xl border border-slate-100">
                <table className="min-w-full divide-y divide-slate-200">
                  <thead className="bg-slate-50">
                    <tr>
                      <th className="px-4 py-3 text-left text-sm font-semibold text-slate-700">#</th>
                      <th className="px-4 py-3 text-left text-sm font-semibold text-slate-700">Từ vựng</th>
                      <th className="px-4 py-3 text-left text-sm font-semibold text-slate-700">Ngày học</th>
                      <th className="px-4 py-3 text-right text-sm font-semibold text-slate-700">Thao tác</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100 bg-white">
                    {filteredWords.map((word, index) => (
                      <tr key={word.id || index}>
                        <td className="px-4 py-3 text-sm text-slate-500">{((currentPage - 1) * pageSize) + index + 1}</td>
                        <td className="px-4 py-3 text-sm font-medium text-slate-800">{word.word}</td>
                        <td className="px-4 py-3 text-sm text-slate-600">{word.learnedAt ? formatDate(word.learnedAt) : '-'}</td>
                        <td className="px-4 py-3">
                          <div className="flex justify-end gap-2">
                            <button
                              type="button"
                              className={`${actionButtonClass} border-sky-300 text-sky-700 hover:bg-sky-50`}
                              onClick={() => viewWordDetail(word.word)}
                              title="Xem chi tiết từ vựng"
                            >
                              <i className="bi bi-eye"></i>
                            </button>
                            <button
                              type="button"
                              className={`${actionButtonClass} border-rose-300 text-rose-700 hover:bg-rose-50`}
                              onClick={() => removeWord(word.id, word.word)}
                              disabled={isLoading}
                              title="Xóa từ vựng"
                            >
                              <i className="bi bi-trash"></i>
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <Pagination 
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={handlePageChange}
              />
            </>
          )}
        </div>
      </div>
    </section>
  );
};

export default LearnedWordsPage;
