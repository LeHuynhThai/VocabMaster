import React from 'react';
import { Container } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { getAvatarText } from '../../utils/helpers';
import { ROUTES, MESSAGES } from '../../utils/constants';
import './Header.css';

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
    <header className="app-header">
      <Container fluid className="header-container">
        {/* App Logo and Brand */}
        <div className="header-brand">
          <Link to={ROUTES.HOME} className="navbar-brand d-flex align-items-center">
            <div className="logo-container">
              <i className="bi bi-book-half"></i>
            </div>
            <span className="brand-text">VocabMaster</span>
          </Link>
        </div>
        
        {/* User Authentication Section */}
        <div className="header-auth">
          {isAuthenticated ? (
            <div className="user-section">
              <div className="user-avatar">
                <i className="bi bi-person-fill"></i>
              </div>
              <div className="user-info-horizontal">
                <span className="user-name-highlight">{user?.name}</span>
                {user?.role === 'Admin' && (
                  <Link to={ROUTES.ADMIN_DASHBOARD} className="admin-link">
                    <i className="bi bi-shield-check me-1"></i>
                    <span>Admin</span>
                  </Link>
                )}
                <button 
                  className="logout-button"
                  onClick={handleLogout}
                  type="button"
                >
                  <i className="bi bi-power"></i> Đăng xuất
                </button>
              </div>
            </div>
          ) : (
            <div className="auth-buttons">
              <Link to={ROUTES.LOGIN} className="login-button">
                <i className="bi bi-box-arrow-in-right me-1"></i> Đăng nhập
              </Link>
              <Link to={ROUTES.REGISTER} className="register-button">
                <i className="bi bi-person-plus me-1"></i> Đăng ký
              </Link>
            </div>
          )}
        </div>
      </Container>
    </header>
  );
};

export default Header; 