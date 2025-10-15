import React from 'react';
import { Container, Card, Row, Col, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '../utils/constants';
import './AdminDashboardPage.css';

const AdminDashboardPage: React.FC = () => {
  const navigate = useNavigate();

  const adminFeatures = [
    {
      title: 'Quản lý từ vựng',
      description: 'Thêm, sửa, xóa từ vựng trong hệ thống',
      icon: 'bi-book',
      route: ROUTES.ADMIN_VOCABULARY,
      color: 'primary'
    }
  ];

  return (
    <Container className="py-4 admin-dashboard-page">
      {/* Header removed per request */}

      {/* Welcome Card */}
      <Card className="mb-4 welcome-card">
        <Card.Body>
          <Row className="align-items-center">
            <Col md={8}>
              <h4 className="mb-2">Chào mừng đến với Admin Dashboard</h4>
            </Col>
          </Row>
        </Card.Body>
      </Card>

      {/* Features Grid */}
      <Row className="justify-content-center">
        {adminFeatures.map((feature, index) => (
          <Col md={12} lg={12} className="mb-3" key={index}>
            <Card 
              className={`feature-card h-100`}
              onClick={() => navigate(feature.route)}
            >
              <Card.Body className="text-center">
                <div className={`feature-icon ${feature.color}`}>
                  <i className={`bi ${feature.icon}`}></i>
                </div>
                <h5 className="feature-title">{feature.title}</h5>
                {/* Description removed per request */}
                <Button variant={feature.color as any} size="sm">
                  <i className="bi bi-arrow-right me-1"></i>
                  Truy cập
                </Button>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>

    </Container>
  );
};

export default AdminDashboardPage;
