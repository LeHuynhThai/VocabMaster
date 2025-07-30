import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { getAvatarText } from '../../utils/helpers';
import { ROUTES } from '../../utils/constants';
import './Sidebar.css';

/**
 * Improved Sidebar component
 * Centered navigation items with better spacing
 */
const Sidebar: React.FC = () => {
  const { isAuthenticated, user } = useAuth();
  const location = useLocation();

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
    <div className="sidebar">
      {/* Sidebar content */}
      <div className="sidebar-content">
        {/* User profile section */}
        {user && (
          <div className="sidebar-profile">
            <div className="avatar-container">
              <div className="avatar">
                {getAvatarText(user.name)}
              </div>
            </div>
            <div className="user-info">
              <p className="user-name">{user.name}</p>
            </div>
          </div>
        )}
        
        {/* Navigation links */}
        <nav className="sidebar-nav">
          <h6 className="nav-title">MENU</h6>
          <ul className="nav-list">
            <li className={`nav-item ${isActive(ROUTES.HOME) ? 'active' : ''}`}>
              <Link to={ROUTES.HOME} className="nav-link">
                <i className="bi bi-house-door"></i>
                <span>Trang chủ</span>
              </Link>
            </li>
            
            <li className={`nav-item ${isActive(ROUTES.WORD_GENERATOR) ? 'active' : ''}`}>
              <Link to={ROUTES.WORD_GENERATOR} className="nav-link">
                <i className="bi bi-book"></i>
                <span>Từ vựng mới</span>
              </Link>
            </li>
            
            <li className={`nav-item ${isActive(ROUTES.LEARNED_WORDS) ? 'active' : ''}`}>
              <Link to={ROUTES.LEARNED_WORDS} className="nav-link">
                <i className="bi bi-journal-check"></i>
                <span>Từ đã học</span>
              </Link>
            </li>

            <li className={`nav-item ${isActive(ROUTES.QUIZ) ? 'active' : ''}`}>
              <Link to={ROUTES.QUIZ} className="nav-link">
                <i className="bi bi-question-circle"></i>
                <span>Trắc nghiệm</span>
              </Link>
            </li>
          </ul>
        </nav>
      </div>
    </div>
  );
};

export default Sidebar; 