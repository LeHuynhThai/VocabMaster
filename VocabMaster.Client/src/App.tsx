import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import Header from './components/layout/Header';
import Sidebar from './components/layout/Sidebar';  
import Footer from './components/layout/Footer'; 
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import WordGeneratorPage from './pages/WordGeneratorPage';
import LearnedWordsPage from './pages/LearnedWordsPage';
import NotFoundPage from './pages/NotFoundPage';
import { ROUTES } from './utils/constants';

/**
 * Protected route component that redirects to login if user is not authenticated
 */
const ProtectedRoute: React.FC<{ element: React.ReactNode }> = ({ element }) => {
  const { isAuthenticated, isLoading } = useAuth();
  
  // While checking authentication status, show nothing
  if (isLoading) {
    return null;
  }
  
  // Redirect to login if not authenticated
  return isAuthenticated ? (
    <>{element}</>
  ) : (
    <Navigate to={ROUTES.LOGIN} replace state={{ message: "Vui lòng đăng nhập để tiếp tục" }} />
  );
};

/**
 * Main App content component
 * Sets up routing and global layout structure
 */
function AppContent() {
  return (
    <Router>
      <div className="app-container">
        <Header />
        <Sidebar />
        <main className="app-main">
          <Routes>
            <Route path={ROUTES.HOME} element={<HomePage />} />
            <Route path={ROUTES.LOGIN} element={<LoginPage />} />
            <Route path={ROUTES.REGISTER} element={<RegisterPage />} />
            <Route 
              path={ROUTES.WORD_GENERATOR} 
              element={<ProtectedRoute element={<WordGeneratorPage />} />} 
            />
            <Route 
              path={ROUTES.LEARNED_WORDS} 
              element={<ProtectedRoute element={<LearnedWordsPage />} />} 
            />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </main>
        <Footer />
      </div>
    </Router>
  );
}

/**
 * Main App component
 * Wraps the app content with providers
 */
function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App; 