import React, { useState, useEffect } from 'react';
import { Container, Card, Form, Button, Alert, Row, Col } from 'react-bootstrap';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useGoogleLogin } from '@react-oauth/google';
import { GoogleAuthRequest } from '../types';

/**
 * Login page component
 */
const LoginPage: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isGoogleLoading, setIsGoogleLoading] = useState(false);
  const { login, googleLogin, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  
  // Get success message from location state (e.g., after registration)
  // only get message if it is not a redirect message
  const successMessage = location.state?.message && 
    !location.state.message.includes("Vui lòng đăng nhập") ? 
    location.state.message : null;

  useEffect(() => {
    // if already authenticated, redirect to home silently
    if (isAuthenticated) {
      navigate('/', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);
    
    try {
      await login({ name: username, password });
      navigate('/', { replace: true });
    } catch (err: any) {
      setError(err.message || 'Đăng nhập thất bại. Vui lòng thử lại.');
    } finally {
      setIsLoading(false);
    }
  };

  // Xử lý đăng nhập Google
  const handleGoogleLogin = useGoogleLogin({
    onSuccess: async (tokenResponse) => {
      setIsGoogleLoading(true);
      try {
        const googleAuth: GoogleAuthRequest = {
          accessToken: tokenResponse.access_token
        };
        await googleLogin(googleAuth);
        navigate('/', { replace: true });
      } catch (err: any) {
        setError(err.message || 'Đăng nhập Google thất bại. Vui lòng thử lại.');
      } finally {
        setIsGoogleLoading(false);
      }
    },
    onError: (errorResponse) => {
      console.error('Google login error:', errorResponse);
      setError('Đăng nhập Google thất bại. Vui lòng thử lại.');
    }
  });

  return (
    <Container className="auth-page">
      <div className="form-container">
        <Card className="border-0 shadow">
          <Card.Body className="p-4">
            <div className="text-center mb-4">
              <h2 className="fw-bold">Đăng nhập</h2>
              <p className="text-muted">Nhập thông tin đăng nhập của bạn</p>
            </div>
            
            {successMessage && (
              <Alert variant="success" className="mb-4">
                {successMessage}
              </Alert>
            )}
            
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
                  placeholder="Nhập tên đăng nhập"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  required
                />
              </Form.Group>
              
              <Form.Group className="mb-4" controlId="password">
                <Form.Label>Mật khẩu</Form.Label>
                <Form.Control
                  type="password"
                  placeholder="Nhập mật khẩu"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
              </Form.Group>
              
              <Button 
                variant="primary" 
                type="submit" 
                className="w-100 py-2 mb-3"
                disabled={isLoading || isGoogleLoading}
              >
                {isLoading ? 'Đang đăng nhập...' : 'Đăng nhập'}
              </Button>

              <div className="text-center mb-3">
                <p className="text-muted mb-2">Hoặc đăng nhập bằng</p>
              </div>
              
              <Button
                variant="outline-danger"
                className="w-100 py-2 d-flex align-items-center justify-content-center"
                onClick={() => handleGoogleLogin()}
                disabled={isLoading || isGoogleLoading}
              >
                <i className="bi bi-google me-2"></i>
                {isGoogleLoading ? 'Đang đăng nhập...' : 'Đăng nhập bằng Google'}
              </Button>
            </Form>
            
            <div className="text-center mt-4">
              <p className="mb-0">
                Chưa có tài khoản? <Link to="/register">Đăng ký</Link>
              </p>
            </div>
          </Card.Body>
        </Card>
      </div>
    </Container>
  );
};

export default LoginPage; 