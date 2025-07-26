import React from 'react';
import { Container, Row, Col } from 'react-bootstrap';
import TranslationBox from '../components/ui/TranslationBox';

/**
 * TranslationPage component
 * Provides a dedicated page for translation functionality
 */
const TranslationPage: React.FC = () => {
  return (
    <Container className="py-4">
      <Row>
        <Col lg={8} className="mx-auto">
          <div className="page-header">
            <h1 className="page-title">Dịch văn bản</h1>
            <p className="page-description">
              Dịch văn bản tiếng Anh sang tiếng Việt với LibreTranslate
            </p>
          </div>
          
          <TranslationBox className="mt-4" />
          
          <div className="mt-4 p-3 bg-light rounded">
            <h5>Hướng dẫn sử dụng</h5>
            <ul>
              <li>Nhập văn bản tiếng Anh vào ô nhập liệu</li>
              <li>Nhấn nút "Dịch" để dịch văn bản sang tiếng Việt</li>
              <li>Kết quả dịch sẽ hiển thị bên dưới</li>
              <li>Nhấn nút "Xóa" để xóa văn bản và bắt đầu lại</li>
            </ul>
            
            <p className="mb-0 text-muted">
              <small>
                Dịch vụ dịch thuật được cung cấp bởi LibreTranslate API.
                Kết quả dịch có thể không hoàn hảo, đặc biệt với các văn bản chuyên ngành.
              </small>
            </p>
          </div>
        </Col>
      </Row>
    </Container>
  );
};

export default TranslationPage; 