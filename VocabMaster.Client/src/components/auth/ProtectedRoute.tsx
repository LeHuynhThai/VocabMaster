import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { ROUTES } from '../../utils/constants';

interface ProtectedRouteProps {
  element: React.ReactNode;
}

/**
 * Protected route component that redirects to login if user is not authenticated
 */
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ element }) => {
  const { isAuthenticated, isLoading } = useAuth();
  
  // While checking authentication status, show loading
  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '100vh' }}>
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }
  
  // Redirect to login if not authenticated
  return isAuthenticated ? (
    <>{element}</>
  ) : (
    <Navigate to={ROUTES.LOGIN} replace state={{ message: "Vui lòng đăng nhập để tiếp tục" }} />
  );
};

export default ProtectedRoute; 