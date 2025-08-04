import React, { createContext, useContext, useState, ReactNode } from 'react';

/**
 * Context for managing quiz statistics refresh
 * Provides a way to trigger refresh of quiz stats from anywhere in the app
 */
interface QuizStatsContextType {
  lastRefresh: number;
  refreshStats: () => void;
}

const QuizStatsContext = createContext<QuizStatsContextType | undefined>(undefined);

/**
 * Provider component for quiz stats context
 */
export const QuizStatsProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  // Use timestamp to track when stats should be refreshed
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now());

  /**
   * Trigger a refresh of quiz statistics
   * Updates the lastRefresh timestamp which components can listen to
   */
  const refreshStats = () => {
    setLastRefresh(Date.now());
  };

  return (
    <QuizStatsContext.Provider value={{ lastRefresh, refreshStats }}>
      {children}
    </QuizStatsContext.Provider>
  );
};

/**
 * Custom hook to use quiz stats context
 * @returns QuizStatsContextType object with lastRefresh timestamp and refreshStats function
 */
export const useQuizStats = (): QuizStatsContextType => {
  const context = useContext(QuizStatsContext);
  if (context === undefined) {
    throw new Error('useQuizStats must be used within a QuizStatsProvider');
  }
  return context;
}; 