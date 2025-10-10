import React from 'react';
import { Row, Col, Card } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { ROUTES } from '../utils/constants';
import './HomePage.css';

/**
 * Home page component
 * Displays the main landing page with app features and call-to-action
 */
const HomePage: React.FC = () => {
  // Get authentication status from context
  const { isAuthenticated, user } = useAuth();

  return (
    <div className="home-container d-flex flex-column justify-content-between">
      {/* Main Content */}
      <div className="d-flex justify-content-center align-items-center position-relative flex-grow-1">
        {/* Background shapes for visual interest */}
        <div className="shape shape-1"></div>
        <div className="shape shape-2"></div>
        <div className="shape shape-3"></div>

        <Card className="shadow-lg border-0 main-card">
          <Card.Body className="p-md-5 p-4">
            {/* App Title and Intro */}
            <div className="text-center mb-5">
              <div className="app-icon mb-4">
                <i className="bi bi-book-half"></i>
              </div>
              <h1 className="display-4 fw-bold mb-3 text-gradient">
                VocabMaster
              </h1>
              <p className="lead mb-4">
                Trang web giúp bạn học từ vựng tiếng Anh mỗi ngày
                <br />
                <br />
                <span className="highlight-text">Dễ dàng - Hiệu quả - Miễn phí</span>
              </p>
            </div>

            {/* Feature Cards */}
            <Row className="g-4 mb-5">
              <Col md={4}>
                <Card className="h-100 feature-card">
                  <Card.Body>
                    <div className="feature-icon-wrapper mb-3">
                      <div className="feature-icon" style={{background: "linear-gradient(45deg, #FF9F1C, #FFBF69)"}}>
                        <i className="bi bi-lightning-charge"></i>
                      </div>
                    </div>
                    <h5 className="feature-title">Học nhanh</h5>
                    <p className="feature-text">Từ vựng mới mỗi ngày giúp bạn cải thiện vốn từ nhanh chóng</p>
                  </Card.Body>
                </Card>
              </Col>
              <Col md={4}>
                <Card className="h-100 feature-card">
                  <Card.Body>
                    <div className="feature-icon-wrapper mb-3">
                      <div className="feature-icon" style={{background: "linear-gradient(45deg, #4CC9F0, #4361EE)"}}>
                        <i className="bi bi-graph-up"></i>
                      </div>
                    </div>
                    <h5 className="feature-title">Theo dõi tiến độ</h5>
                    <p className="feature-text">Xem sự tiến bộ của bạn qua các biểu đồ trực quan</p>
                  </Card.Body>
                </Card>
              </Col>
              <Col md={4}>
                <Card className="h-100 feature-card">
                  <Card.Body>
                    <div className="feature-icon-wrapper mb-3">
                      <div className="feature-icon" style={{background: "linear-gradient(45deg, #7209B7, #B5179E)"}}>
                        <i className="bi bi-trophy"></i>
                      </div>
                    </div>
                    <h5 className="feature-title">Thành tích</h5>
                    <p className="feature-text">Nhận huy hiệu và phần thưởng khi hoàn thành mục tiêu học tập</p>
                  </Card.Body>
                </Card>
              </Col>
            </Row>

            {/* Call to Action Button */}
            <div className="text-center">
              {isAuthenticated ? (
                <Link to={ROUTES.WORD_GENERATOR} className="btn btn-primary btn-lg px-5 py-3 rounded-pill start-button">
                  <i className="bi bi-play-circle me-2"></i>
                  Bắt đầu học từ mới
                </Link>
              ) : (
                <Link to={ROUTES.LOGIN} className="btn btn-primary btn-lg px-5 py-3 rounded-pill start-button">
                  <i className="bi bi-box-arrow-in-right me-2"></i>
                  Đăng nhập để bắt đầu
                </Link>
              )}
            </div>
          </Card.Body>
        </Card>
      </div>
    </div>
  );
};

export default HomePage; 