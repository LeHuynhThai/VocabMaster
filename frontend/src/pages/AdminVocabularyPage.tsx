import React, { useCallback, useEffect, useState } from 'react';
import Pagination from '../components/ui/Pagination';
import adminService, { AddVocabularyRequest, VocabularyResponse } from '../services/adminService';
import useToast from '../hooks/useToast';

const AdminVocabularyPage: React.FC = () => {
  const [showAddModal, setShowAddModal] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const { showToast } = useToast();
  const [vocabularies, setVocabularies] = useState<VocabularyResponse[]>([]);
  const [loadingList, setLoadingList] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState('');

  const [formData, setFormData] = useState<AddVocabularyRequest>({
    word: '',
    vietnamese: '',
    meaningsJson: '[]',
    pronunciationsJson: '[]'
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const cleaned = formData.word.trim().toLowerCase();
    if (!cleaned) {
      setError('Vui lòng nhập từ tiếng Anh');
      return;
    }

    setLoading(true);
    setError('');

    try {
      // Tạm thời tự điền các trường còn lại để lưu nhanh
      await adminService.crawlFromApi(cleaned);
      showToast('Đã lấy dữ liệu và lưu!', 'success');
      setShowAddModal(false);
      resetForm();
      await loadVocabularies();
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Có lỗi xảy ra khi lấy dữ liệu';
      setError(errorMessage);
      showToast(errorMessage, 'danger');
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setFormData({
      word: '',
      vietnamese: '',
      meaningsJson: '[]',
      pronunciationsJson: '[]'
    });
    setError('');
  };

  const handleCloseModal = () => {
    setShowAddModal(false);
    resetForm();
  };

  const filterList = useCallback((list: VocabularyResponse[], keyword: string) => {
    const q = keyword.trim().toLowerCase();
    if (!q) return list;
    return list.filter(v =>
      v.word.toLowerCase().includes(q) || (v.vietnamese || '').toLowerCase().includes(q)
    );
  }, []);

  const loadVocabularies = useCallback(async () => {
    setLoadingList(true);
    try {
      const list = await adminService.getVocabularies();
      setVocabularies(list);
    } catch (error) {
      showToast('Không thể tải danh sách từ vựng', 'danger');
    } finally {
      setLoadingList(false);
    }
  }, [showToast]);

  useEffect(() => {
    loadVocabularies();
  }, [loadVocabularies]);

  useEffect(() => {
    const filtered = filterList(vocabularies, search);
    const pages = Math.max(1, Math.ceil(filtered.length / pageSize));
    setTotalPages(pages);
    if (currentPage > pages) {
      setCurrentPage(1);
    }
  }, [currentPage, filterList, pageSize, search, vocabularies]);

  const filteredVocabularies = filterList(vocabularies, search);

  return (
    <section className="min-h-[calc(100vh-80px)] max-w-6xl bg-transparent px-4 py-8 lg:ml-[250px] lg:border-l lg:border-slate-200 lg:bg-[#f8f9fa]">
      <div className="mb-4 rounded-2xl bg-white p-6 shadow-[0_2px_10px_rgba(0,0,0,0.1)]">
          <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div>
              <h1 className="text-xl font-semibold text-slate-900">Danh sách từ vựng</h1>
              <p className="mt-1 text-sm text-slate-500">Quản lý từ vựng trong hệ thống</p>
            </div>
            <button
              type="button"
              onClick={() => setShowAddModal(true)}
              className="inline-flex items-center rounded-xl border-0 bg-[linear-gradient(135deg,#6f42c1,#8e44ad)] px-4 py-3 font-medium shadow-sm transition hover:-translate-y-0.5 hover:shadow-[0_4px_12px_rgba(111,66,193,0.3)]"
            >
              <i className="bi bi-plus-circle mr-2"></i>
              Thêm từ vựng mới
            </button>
          </div>
      </div>

      <div className="rounded-2xl bg-white p-6 shadow-[0_2px_10px_rgba(0,0,0,0.1)]">
          <div className="mb-3 flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <h2 className="text-lg font-semibold text-slate-900">Danh sách</h2>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <div className="flex overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm focus-within:border-[#6f42c1] focus-within:ring-4 focus-within:ring-[#6f42c1]/10 sm:max-w-[280px]">
                <input
                  placeholder="Tìm kiếm từ hoặc nghĩa..."
                  value={search}
                  onChange={(e) => { setSearch(e.target.value); setCurrentPage(1); }}
                  disabled={loadingList}
                  className="w-full bg-transparent px-4 py-3 text-slate-900 outline-none placeholder:text-slate-400"
                />
                <div className="flex items-center border-l border-slate-200 px-4 text-slate-400">
                  <i className="bi bi-search" />
                </div>
              </div>
              <button
                type="button"
                onClick={loadVocabularies}
                disabled={loadingList}
                className="inline-flex items-center justify-center rounded-xl border border-brand-primary px-4 py-3 text-sm font-medium text-brand-primary transition hover:bg-brand-primary/5 disabled:cursor-not-allowed disabled:opacity-60"
              >
                <i className="bi bi-arrow-clockwise mr-1"></i>
                Làm mới
              </button>
            </div>
          </div>
          {loadingList ? (
            <div className="flex justify-center py-8">
              <div className="h-10 w-10 animate-spin rounded-full border-4 border-slate-200 border-t-brand-primary"></div>
            </div>
          ) : filteredVocabularies.length === 0 ? (
            <div className="py-8 text-center text-slate-500">Chưa có từ vựng</div>
          ) : (
            <div className="overflow-x-auto rounded-2xl border border-slate-100">
              <table className="min-w-full divide-y divide-slate-200">
                <thead className="bg-slate-50">
                  <tr>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-slate-700">#</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-slate-700">Từ</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold text-slate-700">Nghĩa</th>
                    <th className="px-4 py-3 text-right text-sm font-semibold text-slate-700">Thao tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 bg-white">
                  {filteredVocabularies
                    .slice((currentPage - 1) * pageSize, (currentPage - 1) * pageSize + pageSize)
                    .map((v, idx) => (
                    <tr key={v.id}>
                      <td className="px-4 py-3 text-sm text-slate-500">{(currentPage - 1) * pageSize + idx + 1}</td>
                      <td className="px-4 py-3 text-sm font-medium text-slate-800">{v.word}</td>
                      <td className="px-4 py-3 text-sm text-slate-600">{v.vietnamese}</td>
                      <td className="px-4 py-3 text-right">
                        <button
                          type="button"
                          className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-rose-300 text-rose-700 transition hover:bg-rose-50"
                          onClick={async () => {
                            if (!window.confirm(`Xóa từ "${v.word}"?`)) return;
                            try {
                              const ok = await adminService.deleteVocabulary(v.id);
                              if (ok) {
                                showToast('Đã xóa từ vựng', 'success');
                                loadVocabularies();
                              } else {
                                showToast('Không thể xóa', 'danger');
                              }
                            } catch (e) {
                              showToast('Có lỗi khi xóa', 'danger');
                            }
                          }}
                        >
                          <i className="bi bi-trash"></i>
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {totalPages > 1 && (
                <div className="mt-4 flex justify-center">
                  <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    onPageChange={(p: number) => {
                      setCurrentPage(p);
                      window.scrollTo({ top: 0, behavior: 'smooth' });
                    }}
                  />
                </div>
              )}
            </div>
          )}
      </div>

      {showAddModal && (
        <div
          className="fixed inset-0 z-[1100] flex items-center justify-center bg-slate-950/50 p-4"
          onClick={(event) => {
            if (event.target === event.currentTarget && !loading) {
              handleCloseModal();
            }
          }}
        >
          <div className="w-full max-w-2xl overflow-hidden rounded-[1.5rem] bg-white shadow-2xl">
            <div className="flex items-center justify-between bg-[linear-gradient(135deg,#6f42c1,#8e44ad)] px-6 py-5 text-white">
              <h2 className="flex items-center text-xl font-semibold">
                <i className="bi bi-plus-circle mr-2"></i>
                Thêm từ vựng mới
              </h2>
              <button
                type="button"
                onClick={handleCloseModal}
                disabled={loading}
                className="rounded-full p-2 text-white/80 transition hover:bg-white/10 hover:text-white disabled:cursor-not-allowed disabled:opacity-60"
                aria-label="Đóng modal"
              >
                <i className="bi bi-x-lg"></i>
              </button>
            </div>

            <form onSubmit={handleSubmit}>
              <div className="p-8">
            {error && (
              <div className="mb-4 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                {error}
              </div>
            )}

                <div className="space-y-2">
                  <label htmlFor="word" className="block text-sm font-medium text-slate-700">
                Từ tiếng Anh <span className="text-rose-500">*</span>
                  </label>
                  <input
                    id="word"
                type="text"
                name="word"
                value={formData.word}
                onChange={handleInputChange}
                placeholder="Nhập từ tiếng Anh..."
                required
                disabled={loading}
                    className="w-full rounded-xl border-2 border-slate-200 px-4 py-3 text-slate-900 outline-none transition focus:border-[#6f42c1] focus:ring-4 focus:ring-[#6f42c1]/10"
              />
                </div>

            {/* Giữ lại duy nhất trường Từ tiếng Anh */}
              </div>
              <div className="flex flex-col-reverse gap-3 border-t border-slate-200 bg-slate-50 px-8 py-6 sm:flex-row sm:justify-end">
                <button
                  type="button"
                  onClick={handleCloseModal}
                  disabled={loading}
                  className="inline-flex items-center justify-center rounded-xl bg-slate-200 px-4 py-3 font-medium text-slate-700 transition hover:bg-slate-300 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  disabled={loading}
                  className="inline-flex items-center justify-center rounded-xl bg-[linear-gradient(135deg,#6f42c1,#8e44ad)] px-4 py-3 font-medium text-white shadow-sm transition hover:shadow-md disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {loading ? (
                    <>
                      <span className="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent"></span>
                      Đang xử lý...
                    </>
                  ) : (
                    <>
                      <i className="bi bi-check-circle mr-2"></i>
                      Lấy dữ liệu và lưu
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  );
};

export default AdminVocabularyPage;
