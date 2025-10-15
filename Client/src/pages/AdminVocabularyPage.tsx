import React, { useState } from 'react';
import { Container, Card, Button, Modal, Form, Alert, Spinner } from 'react-bootstrap';
import adminService, { AddVocabularyRequest } from '../services/adminService';
import useToast from '../hooks/useToast';
import './AdminVocabularyPage.css';

const AdminVocabularyPage: React.FC = () => {
  const [showAddModal, setShowAddModal] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const { showToast } = useToast();

  const [formData, setFormData] = useState<AddVocabularyRequest>({
    word: '',
    vietnamese: '',
    meaningsJson: '[]',
    pronunciationsJson: '[]'
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.word.trim() || !formData.vietnamese.trim()) {
      setError('Vui lòng điền đầy đủ thông tin từ vựng');
      return;
    }

    setLoading(true);
    setError('');

    try {
      await adminService.addVocabulary(formData);
      showToast('Thêm từ vựng thành công!', 'success');
      setShowAddModal(false);
      resetForm();
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Có lỗi xảy ra khi thêm từ vựng';
      setError(errorMessage);
      showToast(errorMessage, 'danger');
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setFormData({
      word: '',
      vietnamese: '',
      meaningsJson: '[]',
      pronunciationsJson: '[]'
    });
    setError('');
  };

  const handleCloseModal = () => {
    setShowAddModal(false);
    resetForm();
  };

  return (
    <Container className="py-4 admin-vocabulary-page">
      <div className="mb-4">
        <h1 className="page-title">Quản lý từ vựng</h1>
        <p className="text-muted">Thêm và quản lý từ vựng trong hệ thống</p>
      </div>

      {/* Action Buttons */}
      <Card className="mb-4">
        <Card.Body>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h5 className="mb-0">Danh sách từ vựng</h5>
              <small className="text-muted">Quản lý từ vựng trong hệ thống</small>
            </div>
            <Button 
              variant="primary" 
              onClick={() => setShowAddModal(true)}
              className="add-vocabulary-btn"
            >
              <i className="bi bi-plus-circle me-2"></i>
              Thêm từ vựng mới
            </Button>
          </div>
        </Card.Body>
      </Card>

      {/* Placeholder for vocabulary list */}
      <Card>
        <Card.Body>
          <div className="text-center py-5">
            <i className="bi bi-book display-4 text-muted mb-3"></i>
            <h4 className="text-muted">Danh sách từ vựng</h4>
            <p className="text-muted">Chức năng hiển thị danh sách từ vựng sẽ được thêm sau</p>
          </div>
        </Card.Body>
      </Card>

      {/* Add Vocabulary Modal */}
      <Modal show={showAddModal} onHide={handleCloseModal} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>
            <i className="bi bi-plus-circle me-2"></i>
            Thêm từ vựng mới
          </Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSubmit}>
          <Modal.Body>
            {error && (
              <Alert variant="danger" className="mb-3">
                {error}
              </Alert>
            )}

            <Form.Group className="mb-3">
              <Form.Label>
                Từ tiếng Anh <span className="text-danger">*</span>
              </Form.Label>
              <Form.Control
                type="text"
                name="word"
                value={formData.word}
                onChange={handleInputChange}
                placeholder="Nhập từ tiếng Anh..."
                required
                disabled={loading}
              />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>
                Nghĩa tiếng Việt <span className="text-danger">*</span>
              </Form.Label>
              <Form.Control
                type="text"
                name="vietnamese"
                value={formData.vietnamese}
                onChange={handleInputChange}
                placeholder="Nhập nghĩa tiếng Việt..."
                required
                disabled={loading}
              />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Định nghĩa chi tiết (JSON)</Form.Label>
              <Form.Control
                as="textarea"
                rows={3}
                name="meaningsJson"
                value={formData.meaningsJson}
                onChange={handleInputChange}
                placeholder='[{"partOfSpeech": "noun", "definitions": [{"text": "definition", "example": "example sentence"}]}]'
                disabled={loading}
              />
              <Form.Text className="text-muted">
                Định dạng JSON cho các định nghĩa chi tiết (tùy chọn)
              </Form.Text>
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Phát âm (JSON)</Form.Label>
              <Form.Control
                as="textarea"
                rows={2}
                name="pronunciationsJson"
                value={formData.pronunciationsJson}
                onChange={handleInputChange}
                placeholder='[{"text": "/həˈloʊ/", "audio": "audio_url"}]'
                disabled={loading}
              />
              <Form.Text className="text-muted">
                Định dạng JSON cho thông tin phát âm (tùy chọn)
              </Form.Text>
            </Form.Group>
          </Modal.Body>
          <Modal.Footer>
            <Button 
              variant="secondary" 
              onClick={handleCloseModal}
              disabled={loading}
            >
              Hủy
            </Button>
            <Button 
              variant="primary" 
              type="submit"
              disabled={loading}
            >
              {loading ? (
                <>
                  <Spinner animation="border" size="sm" className="me-2" />
                  Đang thêm...
                </>
              ) : (
                <>
                  <i className="bi bi-check-circle me-2"></i>
                  Thêm từ vựng
                </>
              )}
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </Container>
  );
};

export default AdminVocabularyPage;
