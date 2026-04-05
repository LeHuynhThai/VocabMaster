import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { getAvatarText } from '../../utils/helpers';
import { ROUTES, MESSAGES } from '../../utils/constants';

/**
 * Header component with improved design
 */
const Header: React.FC = () => {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  /**
   * Handle user logout and redirect to login page
   * @param e - React mouse event
   */
  const handleLogout = async (e: React.MouseEvent) => {
    e.preventDefault();
    try {
      await logout();
      console.log(MESSAGES.LOGOUT_SUCCESS);
      // Redirect to login page after successful logout
      navigate(ROUTES.LOGIN);
    } catch (error) {
      console.error("Lỗi đăng xuất:", error);
    }
  };

  return (
    <header className="fixed inset-x-0 top-0 z-[1030] h-[70px] bg-[linear-gradient(135deg,#6a11cb_0%,#2575fc_100%)] shadow-md">
      <div className="mx-auto flex h-full w-full items-center justify-between px-4 sm:px-6 lg:px-8">
        <div className="flex items-center">
          <Link to={ROUTES.HOME} className="group flex items-center gap-3 no-underline">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-white/20 text-2xl text-white transition duration-300 group-hover:rotate-12">
              <i className="bi bi-book-half"></i>
            </div>
            <span className="text-xl font-bold tracking-[0.03em] text-white sm:text-2xl">VocabMaster</span>
          </Link>
        </div>

        <div className="ml-auto flex items-center justify-end">
          {isAuthenticated ? (
            <div className="flex items-center rounded-full bg-white/15 px-2 py-1.5 text-white transition duration-300 hover:bg-white/25 sm:px-4">
              <div className="mr-3 flex h-9 w-9 items-center justify-center rounded-full bg-white/30 text-sm font-semibold uppercase text-white shadow-sm">
                {user?.name ? getAvatarText(user.name) : <i className="bi bi-person-fill"></i>}
              </div>
              <div className="flex items-center gap-2 sm:gap-4">
                <span className="max-w-[120px] truncate text-sm font-semibold text-[#ffd700] sm:max-w-[180px]">
                  {user?.name}
                </span>
                <button
                  className="inline-flex items-center gap-2 rounded-full bg-white/20 px-3 py-2 text-xs font-medium text-white transition hover:bg-red-500/20"
                  onClick={handleLogout}
                  type="button"
                >
                  <i className="bi bi-power"></i> Đăng xuất
                </button>
              </div>
            </div>
          ) : (
            <div className="flex items-center gap-2 sm:gap-3">
              <Link
                to={ROUTES.LOGIN}
                className="inline-flex items-center gap-2 rounded-full border border-white/50 px-4 py-2 text-sm font-medium text-white no-underline transition hover:border-white hover:bg-white/10"
              >
                <i className="bi bi-box-arrow-in-right"></i> Đăng nhập
              </Link>
              <Link
                to={ROUTES.REGISTER}
                className="inline-flex items-center gap-2 rounded-full bg-white px-4 py-2 text-sm font-medium text-[#6a11cb] no-underline shadow-sm transition hover:-translate-y-0.5 hover:bg-white/90 hover:shadow-md"
              >
                <i className="bi bi-person-plus"></i> Đăng ký
              </Link>
            </div>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header; 