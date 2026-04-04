import React from 'react';
import { Link } from 'react-router-dom';

/**
 * NotFoundPage component
 * Displayed when a user navigates to a non-existent route
 */
const NotFoundPage: React.FC = () => {
  return (
    <section className="flex min-h-[calc(100vh-220px)] items-center justify-center px-4 py-12">
      <div className="w-full max-w-2xl rounded-[2rem] bg-white p-8 text-center shadow-soft ring-1 ring-slate-200 sm:p-12">
        <p className="bg-brand-gradient-text bg-clip-text text-7xl font-black text-transparent sm:text-8xl">404</p>
        <h1 className="mt-4 text-3xl font-bold text-slate-900 sm:text-4xl">Trang không tồn tại</h1>
        <p className="mx-auto mt-4 max-w-xl text-lg leading-8 text-slate-600">
          Trang bạn đang tìm kiếm không tồn tại hoặc đã bị di chuyển.
        </p>
        <Link
          to="/"
          className="mt-8 inline-flex items-center rounded-full bg-brand-gradient px-6 py-3 text-base font-semibold text-white no-underline shadow-lg transition hover:-translate-y-0.5 hover:shadow-xl"
        >
          <i className="bi bi-house-door mr-2"></i>
          Trở về trang chủ
        </Link>
      </div>
    </section>
  );
};

export default NotFoundPage;