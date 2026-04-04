import React, { useEffect, useState } from 'react';
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
  const [showLoader, setShowLoader] = useState<boolean>(true);
  const [initialCheckDone, setInitialCheckDone] = useState<boolean>(false);
  
  // only show loading spinner after a short delay to avoid UI flickering when loading quickly
  useEffect(() => {
    const timer = setTimeout(() => {
      setShowLoader(false);
    }, 300);
    
    return () => clearTimeout(timer);
  }, []);
  
  // mark when auth check is done
  useEffect(() => {
    if (!isLoading) {
      setInitialCheckDone(true);
    }
  }, [isLoading]);
  
  // when loading and not done yet, show spinner
  if ((isLoading || !initialCheckDone) && showLoader) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '100vh' }}>
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }
  
  // if auth check is done and not authenticated, redirect to login page
  if (initialCheckDone && !isAuthenticated) {
    return <Navigate to={ROUTES.LOGIN} replace state={{ message: "Vui lòng đăng nhập để tiếp tục" }} />;
  }
  
  // if loading is done or authenticated, show content
  return <>{element}</>;
};

export default ProtectedRoute; 