import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';
import Header from './components/Header';
import Sidebar from './components/Sidebar';
import Footer from './components/Footer';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import WordGeneratorPage from './pages/WordGeneratorPage';
import LearnedWordsPage from './pages/LearnedWordsPage';
import NotFoundPage from './pages/NotFoundPage';
import { AuthProvider, useAuth } from './contexts/AuthContext';

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
    <Navigate to="/login" replace state={{ message: "Vui lòng đăng nhập để tiếp tục" }} />
  );
};

/**
 * Main App component
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
            <Route path="/" element={<HomePage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route 
              path="/wordgenerator" 
              element={<ProtectedRoute element={<WordGeneratorPage />} />} 
            />
            <Route 
              path="/learnedwords" 
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

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App; 