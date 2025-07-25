import React, { useState, useEffect } from 'react';
import { Form, Button, Alert, Card, Container, Row, Col } from 'react-bootstrap';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

/**
 * Login page component
 * Handles user authentication using the AuthContext
 */
const LoginPage: React.FC = () => {
  // Form state
  const [name, setName] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  
  // Hooks
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  // Check for success message from redirect (e.g., after registration)
  useEffect(() => {
    // Get success message from location state if available
    const state = location.state as { successMessage?: string } | undefined;
    if (state?.successMessage) {
      setSuccessMessage(state.successMessage);
    }
  }, [location]);

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/');
    }
  }, [isAuthenticated, navigate]);

  /**
   * Handle form submission
   * Attempts to log in the user with the provided credentials
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccessMessage(null);
    setIsSubmitting(true);
    
    try {
      await login({ name, password });
      // Redirect will happen via the useEffect above
    } catch (err: any) {
      setError(err.response?.data?.message || 'Tên đăng nhập hoặc mật khẩu không chính xác');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Container>
      <Row className="justify-content-center">
        <Col md={4}>
          {/* Success message alert */}
          {successMessage && (
            <Alert 
              variant="success" 
              dismissible 
              onClose={() => setSuccessMessage(null)}
              className="mb-4"
            >
              {successMessage}
            </Alert>
          )}

          <Card>
            <Card.Body>
              <h2 className="card-title text-center">Đăng nhập</h2>
              
              {/* Error message */}
              {error && <Alert variant="danger">{error}</Alert>}
              
              <Form onSubmit={handleSubmit}>
                <Form.Group className="mb-3" controlId="formName">
                  <Form.Label>Tên đăng nhập</Form.Label>
                  <Form.Control
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    required
                  />
                </Form.Group>

                <Form.Group className="mb-3" controlId="formPassword">
                  <Form.Label>Mật khẩu</Form.Label>
                  <Form.Control
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                  />
                </Form.Group>

                <div className="text-center">
                  <Button 
                    variant="primary" 
                    type="submit" 
                    disabled={isSubmitting}
                  >
                    {isSubmitting ? 'Đang xử lý...' : 'Đăng nhập'}
                  </Button>
                </div>
              </Form>
              
              <div className="text-center mt-3">
                <p>Chưa có tài khoản? <Link to="/register">Đăng ký ngay</Link></p>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default LoginPage; 