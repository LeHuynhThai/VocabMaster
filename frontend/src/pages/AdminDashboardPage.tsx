import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../utils/constants';

const AdminDashboardPage: React.FC = () => {
  const navigate = useNavigate();

  const adminFeatures = [
    {
      title: 'Quản lý từ vựng',
      icon: 'bi-book',
      route: ROUTES.ADMIN_VOCABULARY,
    }
  ];

  return (
    <section className="min-h-[calc(100vh-80px)] max-w-6xl bg-white px-4 py-4 sm:px-6 lg:ml-[250px] lg:border-l lg:border-slate-200 lg:bg-[#f7f7f8]">
      <div className="mb-4 rounded-2xl border border-slate-200 bg-white p-5 text-slate-700">
        <h4 className="m-0 text-xl font-semibold text-slate-800">Chào mừng đến với Admin Dashboard</h4>
      </div>

      <div className="grid gap-3">
        {adminFeatures.map((feature) => (
          <article
            key={feature.route}
            className="w-full cursor-pointer rounded-2xl border border-slate-200 bg-white p-5 text-center transition duration-150 hover:-translate-y-0.5 hover:shadow-[0_4px_12px_rgba(0,0,0,0.06)]"
            onClick={() => navigate(feature.route)}
          >
            <div className="mx-auto mb-3 flex h-14 w-14 items-center justify-center rounded-full border border-slate-200 bg-slate-100 text-xl text-slate-700">
              <i className={`bi ${feature.icon}`}></i>
            </div>
            <h5 className="mb-3 text-lg font-semibold text-slate-900">{feature.title}</h5>
            <button
              type="button"
              className="inline-flex items-center rounded-md bg-brand-gradient px-3 py-2 text-sm font-medium text-white shadow-sm transition hover:shadow-md"
            >
              <i className="bi bi-arrow-right mr-1"></i>
                  Truy cập
            </button>
          </article>
        ))}
      </div>
    </section>
  );
};

export default AdminDashboardPage;
