import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import ProtectedRoute from '../components/auth/ProtectedRoute';
import AdminDashboardPage from '../pages/AdminDashboardPage';
import AdminVocabularyPage from '../pages/AdminVocabularyPage';
import HomePage from '../pages/HomePage';
import LearnedWordsPage from '../pages/LearnedWordsPage';
import LoginPage from '../pages/LoginPage';
import NotFoundPage from '../pages/NotFoundPage';
import QuizPage from '../pages/QuizPage';
import QuizStatsPage from '../pages/QuizStatsPage';
import RegisterPage from '../pages/RegisterPage';
import WordDetailPage from '../pages/WordDetailPage';
import WordGeneratorPage from '../pages/WordGeneratorPage';
import { ROUTES } from '../utils/constants';

const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to={ROUTES.HOME} />} />
      <Route path={ROUTES.HOME} element={<HomePage />} />
      <Route path={ROUTES.LOGIN} element={<LoginPage />} />
      <Route path={ROUTES.REGISTER} element={<RegisterPage />} />
      <Route
        path={ROUTES.WORD_GENERATOR}
        element={<ProtectedRoute element={<WordGeneratorPage />} />}
      />
      <Route
        path="/word-detail/:word"
        element={<ProtectedRoute element={<WordDetailPage />} />}
      />
      <Route
        path={ROUTES.LEARNED_WORDS}
        element={<ProtectedRoute element={<LearnedWordsPage />} />}
      />
      <Route
        path={ROUTES.QUIZ}
        element={<ProtectedRoute element={<QuizPage />} />}
      />
      <Route
        path={ROUTES.QUIZ_STATS}
        element={<ProtectedRoute element={<QuizStatsPage />} />}
      />
      <Route
        path={ROUTES.ADMIN_VOCABULARY}
        element={<ProtectedRoute element={<AdminVocabularyPage />} />}
      />
      <Route
        path={ROUTES.ADMIN_DASHBOARD}
        element={<ProtectedRoute element={<AdminDashboardPage />} />}
      />
      <Route path={ROUTES.NOT_FOUND} element={<NotFoundPage />} />
      <Route path="*" element={<Navigate to={ROUTES.NOT_FOUND} />} />
    </Routes>
  );
};

export default AppRoutes;