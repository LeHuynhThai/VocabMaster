import React from 'react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

/**
 * Pagination component for navigating between pages
 */
const Pagination: React.FC<PaginationProps> = ({ currentPage, totalPages, onPageChange }) => {
  // Calculate which page numbers to display
  const getPageNumbers = () => {
    const pages = [];
    const maxPagesToShow = 5;
    
    // Always show first page
    pages.push(1);
    
    // Calculate range of pages to show
    let startPage = Math.max(2, currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(totalPages - 1, startPage + maxPagesToShow - 3);
    
    // Adjust if not enough pages
    if (endPage - startPage < maxPagesToShow - 3) {
      startPage = Math.max(2, endPage - (maxPagesToShow - 3));
    }
    
    // Add ellipsis if needed
    if (startPage > 2) {
      pages.push('...');
    }
    
    // Add middle pages
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    
    // Add ellipsis if needed
    if (endPage < totalPages - 1) {
      pages.push('...');
    }
    
    // Always show last page if more than 1 page
    if (totalPages > 1) {
      pages.push(totalPages);
    }
    
    return pages;
  };

  // Don't render pagination if only 1 page
  if (totalPages <= 1) return null;

  const buttonBaseClass =
    'flex h-10 min-w-10 items-center justify-center rounded-md border border-slate-200 bg-white px-3 text-sm text-slate-700 transition';

  return (
    <div className="my-8 flex items-center justify-center gap-2">
      <button
        className={`${buttonBaseClass} disabled:cursor-not-allowed disabled:opacity-50 hover:border-slate-300 hover:bg-slate-50`}
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        aria-label="Previous page"
      >
        <i className="bi bi-chevron-left"></i>
      </button>
      
      {getPageNumbers().map((page, index) => (
        <React.Fragment key={index}>
          {page === '...' ? (
            <span className="flex h-10 min-w-10 items-center justify-center text-slate-400">...</span>
          ) : (
            <button
              className={[
                buttonBaseClass,
                currentPage === page
                  ? 'border-[#6a11cb] bg-[#6a11cb] text-white'
                  : 'hover:border-slate-300 hover:bg-slate-50',
              ].join(' ')}
              onClick={() => typeof page === 'number' && onPageChange(page)}
              aria-current={currentPage === page ? 'page' : undefined}
            >
              {page}
            </button>
          )}
        </React.Fragment>
      ))}
      
      <button
        className={`${buttonBaseClass} disabled:cursor-not-allowed disabled:opacity-50 hover:border-slate-300 hover:bg-slate-50`}
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        aria-label="Next page"
      >
        <i className="bi bi-chevron-right"></i>
      </button>
    </div>
  );
};

export default Pagination; 