import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { getAvatarText } from '../../utils/helpers';
import { ROUTES } from '../../utils/constants';

/**
 * Improved Sidebar component
 * Centered navigation items with better spacing
 */
const Sidebar: React.FC = () => {
  const { isAuthenticated, user } = useAuth();
  const location = useLocation();

  const navigationItems = [
    { label: 'Trang chủ', icon: 'bi-house-door', path: ROUTES.HOME },
    { label: 'Từ vựng mới', icon: 'bi-book', path: ROUTES.WORD_GENERATOR },
    { label: 'Từ đã học', icon: 'bi-journal-check', path: ROUTES.LEARNED_WORDS },
    { label: 'Trắc nghiệm', icon: 'bi-question-circle', path: ROUTES.QUIZ },
    { label: 'Thống kê', icon: 'bi-bar-chart', path: ROUTES.QUIZ_STATS },
  ];

  /**
   * Check if the current path matches the given path
   * @param path - Route path to check
   * @returns boolean indicating if path is active
   */
  const isActive = (path: string) => location.pathname === path;

  // Only show sidebar for authenticated users
  if (!isAuthenticated) {
    return null;
  }

  return (
    <aside className="fixed left-0 top-[70px] z-[1030] hidden h-[calc(100vh-70px)] w-[250px] flex-col overflow-hidden bg-white shadow-sm lg:flex">
      <div className="flex h-full flex-col overflow-y-auto py-6">
        {user && (
          <div className="mb-6 flex items-center border-b border-slate-200 px-5 pb-6">
            <div className="shrink-0">
              <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-[linear-gradient(135deg,#6a11cb_0%,#2575fc_100%)] text-xl font-bold text-white shadow-md">
                {getAvatarText(user.name)}
              </div>
            </div>
            <div className="ml-4 min-w-0">
              <p className="mb-1 truncate text-base font-semibold text-slate-800">{user.name}</p>
            </div>
          </div>
        )}

        <nav className="mb-6">
          <h6 className="px-5 text-xs font-bold uppercase tracking-[0.2em] text-slate-700">Menu</h6>
          <ul className="mt-4 space-y-2">
            {navigationItems.map((item) => {
              const active = isActive(item.path);

              return (
                <li key={item.path}>
                  <Link
                    to={item.path}
                    className={[
                      'group flex items-center gap-4 border-l-[3px] px-5 py-4 font-semibold no-underline transition',
                      active
                        ? 'border-l-[#6a11cb] bg-violet-50 text-[#6a11cb]'
                        : 'border-l-transparent text-slate-700 hover:bg-violet-50/70 hover:text-[#6a11cb]',
                    ].join(' ')}
                  >
                    <i
                      className={[
                        `bi ${item.icon}`,
                        'min-w-6 text-center text-xl',
                        active ? 'text-[#6a11cb]' : 'text-[#6a11cb] group-hover:text-[#6a11cb]',
                      ].join(' ')}
                    ></i>
                    <span>{item.label}</span>
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>
      </div>
    </aside>
  );
};

export default Sidebar; 