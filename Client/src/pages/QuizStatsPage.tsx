import React, { useState, useEffect } from 'react';
import { Container, Card, Row, Col, ProgressBar, ListGroup, Alert, Spinner } from 'react-bootstrap';
import quizService, { QuizStats, CompletedQuiz } from '../services/quizService';
import useToast from '../hooks/useToast';
import './QuizStatsPage.css';

const QuizStatsPage: React.FC = () => {
  const [stats, setStats] = useState<QuizStats | null>(null);
  const [correctAnswers, setCorrectAnswers] = useState<CompletedQuiz[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const { showToast } = useToast();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    setError('');
    
    try {
      // Load stats first
      const statsData = await quizService.getQuizStats();
      setStats(statsData);
      
      // Then load completed answers (separate try-catch to avoid blocking stats)
      try {
        const completedAnswersData = await quizService.getCompletedAnswers();
        setCorrectAnswers(completedAnswersData);
      } catch (completedAnswersError: any) {
        console.error('Error loading completed answers:', completedAnswersError);
        // Don't show error toast for completed answers, just log it
        setCorrectAnswers([]);
      }
    } catch (err: any) {
      console.error('Error loading quiz stats:', err);
      const errorMessage = err.response?.data?.message || 'Không thể tải thống kê. Vui lòng thử lại sau.';
      setError(errorMessage);
      showToast(errorMessage, 'danger');
    } finally {
      setLoading(false);
    }
  };

  const formatPercentage = (value: number): string => {
    return value.toFixed(1) + '%';
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getCompletionPercentage = (): number => {
    if (!stats || stats.totalQuestions === 0) return 0;
    return (stats.completedQuestions / stats.totalQuestions) * 100;
  };

  if (loading) {
    return (
      <Container className="py-4">
        <div className="text-center py-5">
          <Spinner animation="border" role="status" variant="primary">
            <span className="visually-hidden">Đang tải...</span>
          </Spinner>
          <p className="mt-3">Đang tải thống kê...</p>
        </div>
      </Container>
    );
  }

  if (error) {
    return (
      <Container className="py-4">
        <Alert variant="danger">
          <Alert.Heading>Lỗi</Alert.Heading>
          <p>{error}</p>
          <button className="btn btn-outline-danger" onClick={loadData}>
            Thử lại
          </button>
        </Alert>
      </Container>
    );
  }

  return (
    <Container className="py-4 quiz-stats-page">
      <div className="mb-4">
        <h1 className="page-title">Thống kê trắc nghiệm</h1>
        <p className="text-muted">Theo dõi tiến độ học tập của bạn</p>
      </div>

      {/* Summary Cards */}
      <Row className="mb-4">
        <Col md={3} className="mb-3">
          <Card className="stats-card">
            <Card.Body className="text-center">
              <div className="stats-number">{stats?.totalQuestions || 0}</div>
              <div className="stats-label">Tổng số câu hỏi</div>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3} className="mb-3">
          <Card className="stats-card">
            <Card.Body className="text-center">
              <div className="stats-number">{stats?.completedQuestions || 0}</div>
              <div className="stats-label">Đã hoàn thành</div>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3} className="mb-3">
          <Card className="stats-card">
            <Card.Body className="text-center">
              <div className="stats-number">{stats?.correctAnswers || 0}</div>
              <div className="stats-label">Trả lời đúng</div>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3} className="mb-3">
          <Card className="stats-card">
            <Card.Body className="text-center">
              <div className="stats-number">{formatPercentage(stats?.accuracyRate || 0)}</div>
              <div className="stats-label">Tỷ lệ chính xác</div>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      {/* Progress Section */}
      <Card className="mb-4">
        <Card.Body>
          <div className="d-flex justify-content-between align-items-center mb-2">
            <h5 className="mb-0">Tiến độ hoàn thành</h5>
            <span className="text-muted">
              {formatPercentage(getCompletionPercentage())} ({stats?.completedQuestions || 0}/{stats?.totalQuestions || 0})
            </span>
          </div>
          <ProgressBar 
            now={getCompletionPercentage()} 
            variant="primary" 
            className="progress-custom"
          />
        </Card.Body>
      </Card>

      {/* Correct Answers List */}
      <Card>
        <Card.Body>
          <div className="d-flex justify-content-between align-items-center mb-3">
            <h5 className="mb-0">Danh sách các câu đã hoàn thành</h5>
            <button 
              className="btn btn-outline-primary btn-sm" 
              onClick={loadData}
              disabled={loading}
            >
              <i className="bi bi-arrow-clockwise me-1"></i>
              Làm mới
            </button>
          </div>
          
          {correctAnswers.length === 0 ? (
            <div className="text-center py-4">
              <i className="bi bi-inbox display-4 text-muted mb-3"></i>
              <p className="text-muted">Bạn chưa hoàn thành câu hỏi nào.</p>
            </div>
          ) : (
            <ListGroup variant="flush">
              {correctAnswers.map((answer, index) => (
                <ListGroup.Item key={answer.id} className="correct-answer-item">
                  <div className="d-flex justify-content-between align-items-start">
                    <div className="flex-grow-1">
                      <div className="d-flex align-items-center mb-1">
                        <span className={`badge me-2 ${answer.wasCorrect ? 'bg-success' : 'bg-danger'}`}>
                          #{index + 1}
                        </span>
                        <strong className="word-text">{answer.word}</strong>
                      </div>
                      <div className={`answer-text ${answer.wasCorrect ? 'text-success' : 'text-danger'}`}>
                        <i className={`bi me-1 ${answer.wasCorrect ? 'bi-check-circle-fill' : 'bi-x-circle-fill'}`}></i>
                        {answer.correctAnswer}
                      </div>
                    </div>
                    <div className="text-muted small">
                      {formatDate(answer.completedAt)}
                    </div>
                  </div>
                </ListGroup.Item>
              ))}
            </ListGroup>
          )}
        </Card.Body>
      </Card>
    </Container>
  );
};

export default QuizStatsPage;