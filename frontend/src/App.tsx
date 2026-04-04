import React from 'react';
import { BrowserRouter as Router } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { ToastProvider } from './contexts/ToastContext';
import { QuizStatsProvider } from './contexts/QuizStatsContext';
import MainLayout from './layouts/MainLayout';
import AppRoutes from './routes/AppRoutes';

function App() {
  return (
    <Router>
      <AuthProvider>
        <ToastProvider>
          <QuizStatsProvider>
            <MainLayout>
              <AppRoutes />
            </MainLayout>
          </QuizStatsProvider>
        </ToastProvider>
      </AuthProvider>
    </Router>
  );
}

export default App; 