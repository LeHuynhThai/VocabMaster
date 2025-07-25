import React, { useState, useEffect } from 'react';
import { Form, Button, Alert, Card, Container, Row, Col } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

/**
 * Registration page component
 * Allows users to create a new account
 */
const RegisterPage: React.FC = () => {
  // Form state
  const [name, setName] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  // Hooks
  const { register, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/');
    }
  }, [isAuthenticated, navigate]);

  /**
   * Handle form submission
   * Attempts to register a new user with the provided credentials
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsSubmitting(true);
    
    try {
      const success = await register({ name, password });
      
      if (success) {
        // Redirect to login page with success message
        navigate('/login', { 
          state: { successMessage: 'Đăng ký thành công' } 
        });
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Tên đăng nhập đã tồn tại');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Container>
      <Row className="justify-content-center">
        <Col md={4}>
          <Card>
            <Card.Body>
              <h2 className="card-title text-center">Đăng ký tài khoản</h2>
              
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
                    minLength={3}
                    maxLength={50}
                  />
                  <Form.Text className="text-muted">
                    Tên đăng nhập có độ dài từ 3-50 ký tự
                  </Form.Text>
                </Form.Group>

                <Form.Group className="mb-3" controlId="formPassword">
                  <Form.Label>Mật khẩu</Form.Label>
                  <Form.Control
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                    minLength={3}
                    maxLength={8}
                  />
                  <Form.Text className="text-muted">
                    Mật khẩu có độ dài từ 3-8 ký tự
                  </Form.Text>
                </Form.Group>

                <div className="text-center">
                  <Button 
                    variant="primary" 
                    type="submit" 
                    disabled={isSubmitting}
                  >
                    {isSubmitting ? 'Đang xử lý...' : 'Đăng ký'}
                  </Button>
                </div>
              </Form>
              
              <div className="text-center mt-3">
                <p>Đã có tài khoản? <Link to="/login">Đăng nhập</Link></p>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default RegisterPage; 