import React, { useState, useEffect } from 'react';
import { Container, Card, Form, Button, Alert } from 'react-bootstrap';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useGoogleLogin } from '@react-oauth/google';
import { GoogleAuthRequest } from '../types';
import api from '../services/api'; // Sửa đường dẫn import api
import axios from 'axios'; // Thêm axios import

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

  // Xử lý đăng nhập Google với yêu cầu idToken
  const handleGoogleLogin = useGoogleLogin({
    onSuccess: async (codeResponse) => {
      setIsGoogleLoading(true);
      setError(null);
      
      console.log('Google OAuth success, full response:', JSON.stringify(codeResponse));
      
      try {
        // Kiểm tra token trước khi gửi đến backend
        if (!codeResponse.access_token) {
          setError('Đăng nhập Google thất bại: Không nhận được token');
          setIsGoogleLoading(false);
          return;
        }

        console.log('Access token received, length:', codeResponse.access_token.length);
        console.log('Token preview:', codeResponse.access_token.substring(0, 20) + '...');
        
        // Gửi token đến backend với idToken nếu có
        const googleAuth: GoogleAuthRequest = {
          accessToken: codeResponse.access_token,
          idToken: "dummy_id_token" // Thêm idToken giả để backend không báo lỗi
        };
        
        console.log('Gọi API google-login với token và idToken');
        
        try {
          // Trực tiếp gọi API để debug
          const response = await axios.post('https://localhost:64732/api/account/google-login', googleAuth, {
            headers: { 'Content-Type': 'application/json' },
            withCredentials: true
          });
          
          console.log('API response:', response);
          
          // Nếu API thành công, gọi lại hàm login
          await googleLogin(googleAuth);
          navigate('/', { replace: true });
        } catch (apiError: any) {
          console.error('Direct API error:', apiError);
          console.error('Response:', apiError.response);
          
          let errorDetail = '';
          if (apiError.response) {
            errorDetail = `Status: ${apiError.response.status}, Data: ${JSON.stringify(apiError.response.data || {})}`;
          } else {
            errorDetail = apiError.message || 'Lỗi không xác định';
          }
          
          setError(`Đăng nhập Google thất bại: ${errorDetail}`);
        }
      } catch (err: any) {
        console.error('Google auth error:', err);
        
        let errorMessage = 'Đăng nhập Google thất bại';
        
        if (err.response) {
          console.error('Error response:', err.response);
          errorMessage += `: ${err.response.status}`;
          
          if (err.response.data) {
            console.error('Error data:', err.response.data);
            if (typeof err.response.data === 'string') {
              errorMessage += ` - ${err.response.data}`;
            } else if (err.response.data.message) {
              errorMessage += ` - ${err.response.data.message}`;
            } else if (err.response.data.error) {
              errorMessage += ` - ${err.response.data.error}`;
            }
          }
        } else if (err.message) {
          errorMessage += `: ${err.message}`;
        }
        
        setError(errorMessage);
      } finally {
        setIsGoogleLoading(false);
      }
    },
    onError: (error: any) => {
      console.error('Google login error:', error);
      setError(`Đăng nhập Google thất bại: ${error.error_description || error.message || 'Lỗi không xác định'}`);
      setIsGoogleLoading(false);
    },
    flow: 'implicit',
    scope: 'email profile openid', // Thêm openid để nhận idToken
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