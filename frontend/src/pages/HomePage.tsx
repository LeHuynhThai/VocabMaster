import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { ROUTES } from '../utils/constants';

/**
 * Home page component
 * Displays the main landing page with app features and call-to-action
 */
const HomePage: React.FC = () => {
  const { isAuthenticated } = useAuth();

  const features = [
    {
      icon: 'bi-lightning-charge',
      title: 'Học nhanh',
      description: 'Từ vựng mới mỗi ngày giúp bạn cải thiện vốn từ nhanh chóng',
      gradient: 'from-[#FF9F1C] to-[#FFBF69]',
    },
    {
      icon: 'bi-graph-up',
      title: 'Theo dõi tiến độ',
      description: 'Xem sự tiến bộ của bạn qua các biểu đồ trực quan',
      gradient: 'from-[#4CC9F0] to-[#4361EE]',
    },
    {
      icon: 'bi-trophy',
      title: 'Thành tích',
      description: 'Nhận huy hiệu và phần thưởng khi hoàn thành mục tiêu học tập',
      gradient: 'from-[#7209B7] to-[#B5179E]',
    },
  ];

  return (
    <div className={`relative flex min-h-[calc(100vh-140px)] items-center justify-center overflow-hidden py-10 ${isAuthenticated ? 'lg:pl-[250px]' : ''}`}>
      <div className="pointer-events-none absolute left-[10%] top-[-100px] h-[300px] w-[300px] rounded-full bg-[linear-gradient(to_right,#4CC9F0,#4361EE)] opacity-60 blur-[70px] animate-float"></div>
      <div className="pointer-events-none absolute bottom-[-150px] right-[10%] h-[400px] w-[400px] rounded-full bg-[linear-gradient(to_right,#FF9F1C,#FFBF69)] opacity-60 blur-[70px] animate-float-reverse"></div>
      <div className="pointer-events-none absolute bottom-[30%] left-[5%] h-[200px] w-[200px] rounded-full bg-[linear-gradient(to_right,#7209B7,#B5179E)] opacity-60 blur-[70px] animate-float-delayed"></div>

      <section className="relative z-10 w-full max-w-5xl rounded-[2rem] bg-white/95 p-6 shadow-2xl ring-1 ring-white/60 backdrop-blur md:p-10">
        <div className="mb-10 text-center">
          <div className="mx-auto mb-5 flex h-20 w-20 items-center justify-center rounded-full bg-sky-100 text-5xl text-[#4361EE] shadow-inner">
            <i className="bi bi-book-half"></i>
          </div>
          <h1 className="mb-4 bg-brand-gradient-text bg-clip-text text-5xl font-black tracking-tight text-transparent md:text-6xl">
            VocabMaster
          </h1>
          <p className="mx-auto max-w-3xl text-lg leading-8 text-slate-600 md:text-xl">
            Trang web giúp bạn học từ vựng tiếng Anh mỗi ngày
            <span className="mt-4 block text-xl font-semibold text-[#4361EE] md:text-2xl">
              Dễ dàng - Hiệu quả - Miễn phí
            </span>
          </p>
        </div>

        <div className="mb-10 grid gap-4 md:grid-cols-3">
          {features.map((feature) => (
            <article
              key={feature.title}
              className="h-full rounded-2xl border border-slate-100 bg-white p-6 shadow-soft transition duration-300 hover:-translate-y-2 hover:shadow-float"
            >
              <div className="mb-4 flex justify-center">
                <div className={`flex h-[60px] w-[60px] items-center justify-center rounded-full bg-gradient-to-r ${feature.gradient} text-2xl text-white`}>
                  <i className={`bi ${feature.icon}`}></i>
                </div>
              </div>
              <h2 className="mb-3 text-center text-xl font-semibold text-slate-900">{feature.title}</h2>
              <p className="m-0 text-center text-slate-600">{feature.description}</p>
            </article>
          ))}
        </div>

        <div className="text-center">
          <Link
            to={isAuthenticated ? ROUTES.WORD_GENERATOR : ROUTES.LOGIN}
            className="inline-flex items-center rounded-full bg-brand-gradient px-8 py-4 text-lg font-semibold text-white no-underline shadow-lg transition duration-300 hover:-translate-y-1 hover:shadow-xl"
          >
            <i className={`bi ${isAuthenticated ? 'bi-play-circle' : 'bi-box-arrow-in-right'} mr-2`}></i>
            {isAuthenticated ? 'Bắt đầu học từ mới' : 'Đăng nhập để bắt đầu'}
          </Link>
        </div>
      </section>
    </div>
  );
};

export default HomePage; 