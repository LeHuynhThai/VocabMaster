import React, { useState, useEffect } from 'react';
import { Container, Card, Form, Button, Alert } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

/**
 * Register page component
 */
const RegisterPage: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const { register, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    // If already authenticated, redirect to home
    if (isAuthenticated) {
      navigate('/');
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    
    // Validate passwords match
    if (password !== confirmPassword) {
      setError('Mật khẩu không khớp');
      return;
    }
    
    setIsLoading(true);
    
    try {
      const success = await register({ name: username, password });
      if (success) {
        // Redirect to login with success message
        navigate('/login', { 
          state: { message: 'Đăng ký thành công! Vui lòng đăng nhập.' }
        });
      }
    } catch (err: any) {
      setError(err.message || 'Đăng ký thất bại. Vui lòng thử lại.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Container className="auth-page">
      <div className="form-container">
        <Card className="border-0 shadow">
          <Card.Body className="p-4">
            <div className="text-center mb-4">
              <h2 className="fw-bold">Đăng ký</h2>
              <p className="text-muted">Tạo tài khoản mới</p>
            </div>
            
            {error && (
              <Alert variant="danger" className="mb-4">
                {error}
              </Alert>
            )}
            
            <Form onSubmit={handleSubmit}>
              <Form.Group className="mb-3" controlId="username">
                <Form.Label>Tên đăng nhập</Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Chọn tên đăng nhập"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  required
                />
              </Form.Group>
              
              <Form.Group className="mb-3" controlId="password">
                <Form.Label>Mật khẩu</Form.Label>
                <Form.Control
                  type="password"
                  placeholder="Nhập mật khẩu"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
              </Form.Group>
              
              <Form.Group className="mb-4" controlId="confirmPassword">
                <Form.Label>Xác nhận mật khẩu</Form.Label>
                <Form.Control
                  type="password"
                  placeholder="Nhập lại mật khẩu"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  required
                />
              </Form.Group>
              
              <Button 
                variant="primary" 
                type="submit" 
                className="w-100 py-2"
                disabled={isLoading}
              >
                {isLoading ? 'Đang xử lý...' : 'Đăng ký'}
              </Button>
            </Form>
            
            <div className="text-center mt-4">
              <p className="mb-0">
                Đã có tài khoản? <Link to="/login">Đăng nhập</Link>
              </p>
            </div>
          </Card.Body>
        </Card>
      </div>
    </Container>
  );
};

export default RegisterPage; 