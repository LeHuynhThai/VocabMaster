import React from 'react';
import { Container } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
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
      console.log("Đăng xuất thành công");
      // Redirect to login page after successful logout
      navigate('/login');
    } catch (error) {
      console.error("Lỗi đăng xuất:", error);
    }
  };

  return (
    <header className="app-header">
      <Container fluid className="header-container">
        {/* App Logo and Brand */}
        <div className="header-brand">
          <Link to="/" className="navbar-brand d-flex align-items-center">
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
              <Link to="/login" className="login-button">
                <i className="bi bi-box-arrow-in-right me-1"></i> Đăng nhập
              </Link>
              <Link to="/register" className="register-button">
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