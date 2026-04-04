import React from 'react';
import { Container, Row, Col } from 'react-bootstrap';
import { Link } from 'react-router-dom';

/**
 * NotFoundPage component
 * Displayed when a user navigates to a non-existent route
 */
const NotFoundPage: React.FC = () => {
  return (
    <Container className="text-center py-5">
      <Row className="justify-content-center">
        <Col md={8} lg={6}>
          <h1 className="display-1">404</h1>
          <h2 className="mb-4">Trang không tồn tại</h2>
          <p className="lead mb-5">
            Trang bạn đang tìm kiếm không tồn tại hoặc đã bị di chuyển.
          </p>
          <Link to="/" className="btn btn-primary btn-lg">
            <i className="bi bi-house-door me-2"></i>
            Trở về trang chủ
          </Link>
        </Col>
      </Row>
    </Container>
  );
};

export default NotFoundPage; 