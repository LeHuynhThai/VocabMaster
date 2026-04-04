import React from 'react';

/**
 * Footer component for the application
 * Contains copyright information
 */
const Footer: React.FC = () => {
  // Get current year for copyright
  const currentYear = new Date().getFullYear();
  
  return (
    <footer className="mt-8 border-t border-slate-200 bg-slate-50">
      <div className="mx-auto flex h-[70px] w-full max-w-7xl flex-col items-center justify-center px-4 text-center text-sm text-slate-600 sm:px-6 lg:px-8">
        <div className="mb-1 flex items-center gap-2 text-lg font-bold text-[#6a11cb]">
          <i className="bi bi-book-half"></i>
            VocabMaster
        </div>

        <p className="m-0">&copy; {currentYear} VocabMaster. All rights reserved.</p>
        </div>
    </footer>
  );
};

export default Footer; 