import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const WordSearch: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const navigate = useNavigate();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const term = searchTerm.trim();
    if (!term) return;
    navigate(`/word-detail/${encodeURIComponent(term)}`);
  };

  return (
    <form onSubmit={handleSearch} className="mb-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <input
          type="text"
          placeholder="Tìm kiếm từ vựng..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-brand-primary focus:ring-4 focus:ring-brand-primary/10"
        />
        <button
          type="submit"
          className="inline-flex items-center justify-center rounded-2xl bg-brand-gradient px-4 py-3 font-semibold text-white shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
        >
          <i className="bi bi-search"></i>
        </button>
      </div>
    </form>
  );
};

export default WordSearch;


