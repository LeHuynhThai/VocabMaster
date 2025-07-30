import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { ToastProvider } from './contexts/ToastContext';
import ProtectedRoute from './components/auth/ProtectedRoute';
import { useToast } from './contexts/ToastContext';

// Components
import Header from './components/layout/Header';
import Sidebar from './components/layout/Sidebar';
import Footer from './components/layout/Footer';
import ToastContainer from './components/ui/ToastContainer';

// Pages
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import WordGeneratorPage from './pages/WordGeneratorPage';
import LearnedWordsPage from './pages/LearnedWordsPage';
import QuizPage from './pages/QuizPage';
import NotFoundPage from './pages/NotFoundPage';
import { ROUTES } from './utils/constants';

// Styles
import './App.css';

// Toast container wrapper to access context
const ToastContainerWithContext = () => {
  const { toasts, removeToast } = useToast();
  return <ToastContainer toasts={toasts} removeToast={removeToast} />;
};

function App() {
  return (
    <Router>
      <AuthProvider>
        <ToastProvider>
          <div className="app-container">
            <Header />
            <div className="content-wrapper">
              <Sidebar />
              <main className="main-content">
                <Routes>
                  <Route path="/" element={<Navigate to={ROUTES.HOME} />} />
                  <Route path={ROUTES.HOME} element={<HomePage />} />
                  <Route path={ROUTES.LOGIN} element={<LoginPage />} />
                  <Route path={ROUTES.REGISTER} element={<RegisterPage />} />
                  <Route 
                    path={ROUTES.WORD_GENERATOR} 
                    element={
                      <ProtectedRoute element={<WordGeneratorPage />} />
                    } 
                  />
                  <Route 
                    path={ROUTES.LEARNED_WORDS} 
                    element={
                      <ProtectedRoute element={<LearnedWordsPage />} />
                    } 
                  />
                  <Route 
                    path={ROUTES.QUIZ} 
                    element={
                      <ProtectedRoute element={<QuizPage />} />
                    } 
                  />
                  <Route path={ROUTES.NOT_FOUND} element={<NotFoundPage />} />
                  <Route path="*" element={<Navigate to={ROUTES.NOT_FOUND} />} />
                </Routes>
              </main>
            </div>
            <Footer />
            <ToastContainerWithContext />
          </div>
        </ToastProvider>
      </AuthProvider>
    </Router>
  );
}

export default App; 