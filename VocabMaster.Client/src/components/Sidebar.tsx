import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
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
                {user.name.charAt(0).toUpperCase()}
              </div>
            </div>
            <div className="user-info">
              <p className="user-name">{user.name}</p>
            </div>
          </div>
        )}
        
        {/* Navigation links */}
        <nav className="sidebar-nav">
          <h6 className="nav-title">Menu</h6>
          <ul className="nav-list">
            <li className={`nav-item ${isActive('/') ? 'active' : ''}`}>
              <Link to="/" className="nav-link">
                <i className="bi bi-house-door"></i>
                <span>Trang chủ</span>
              </Link>
            </li>
            
            <li className={`nav-item ${isActive('/wordgenerator') ? 'active' : ''}`}>
              <Link to="/wordgenerator" className="nav-link">
                <i className="bi bi-translate"></i>
                <span>Từ vựng mới</span>
              </Link>
            </li>
            
            <li className={`nav-item ${isActive('/learnedwords') ? 'active' : ''}`}>
              <Link to="/learnedwords" className="nav-link">
                <i className="bi bi-journal-text"></i>
                <span>Từ đã học</span>
              </Link>
            </li>
          </ul>
        </nav>
      </div>
    </div>
  );
};

export default Sidebar; 