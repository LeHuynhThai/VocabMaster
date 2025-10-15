import React, { useEffect, useState } from 'react';
import { Container, Card, Button, Modal, Form, Alert, Spinner, Table } from 'react-bootstrap';
import Pagination from '../components/ui/Pagination';
import adminService, { AddVocabularyRequest, VocabularyResponse } from '../services/adminService';
import useToast from '../hooks/useToast';
import './AdminVocabularyPage.css';

const AdminVocabularyPage: React.FC = () => {
  const [showAddModal, setShowAddModal] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const { showToast } = useToast();
  const [vocabularies, setVocabularies] = useState<VocabularyResponse[]>([]);
  const [loadingList, setLoadingList] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);

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
    
    if (!formData.word.trim()) {
      setError('Vui lòng nhập từ tiếng Anh');
      return;
    }

    setLoading(true);
    setError('');

    try {
      // Tạm thời tự điền các trường còn lại để lưu nhanh
      const payload: AddVocabularyRequest = {
        word: formData.word.trim(),
        vietnamese: formData.vietnamese?.trim() || formData.word.trim(),
        meaningsJson: '[]',
        pronunciationsJson: '[]'
      };

      await adminService.addVocabulary(payload);
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

  const loadVocabularies = async () => {
    setLoadingList(true);
    try {
      const list = await adminService.getVocabularies();
      setVocabularies(list);
      const pages = Math.max(1, Math.ceil(list.length / pageSize));
      setTotalPages(pages);
      if (currentPage > pages) setCurrentPage(1);
    } catch (error) {
      showToast('Không thể tải danh sách từ vựng', 'danger');
    } finally {
      setLoadingList(false);
    }
  };

  useEffect(() => {
    loadVocabularies();
  }, []);

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

      {/* Vocabulary list */}
      <Card>
        <Card.Body>
          <div className="d-flex justify-content-between align-items-center mb-3">
            <h5 className="mb-0">Danh sách</h5>
            <Button variant="outline-primary" size="sm" onClick={loadVocabularies} disabled={loadingList}>
              <i className="bi bi-arrow-clockwise me-1"></i>
              Làm mới
            </Button>
          </div>
          {loadingList ? (
            <div className="text-center py-4">
              <Spinner animation="border" role="status" />
            </div>
          ) : vocabularies.length === 0 ? (
            <div className="text-center py-4 text-muted">Chưa có từ vựng</div>
          ) : (
            <div className="table-responsive">
              <Table hover>
                <thead className="table-light">
                  <tr>
                    <th>#</th>
                    <th>Từ</th>
                    <th>Nghĩa</th>
                    <th className="text-end">Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {vocabularies
                    .slice((currentPage - 1) * pageSize, (currentPage - 1) * pageSize + pageSize)
                    .map((v, idx) => (
                    <tr key={v.id}>
                      <td>{(currentPage - 1) * pageSize + idx + 1}</td>
                      <td>{v.word}</td>
                      <td>{v.vietnamese}</td>
                      <td className="text-end">
                        <Button
                          variant="outline-danger"
                          size="sm"
                          onClick={async () => {
                            if (!window.confirm(`Xóa từ "${v.word}"?`)) return;
                            try {
                              const ok = await adminService.deleteVocabulary(v.id);
                              if (ok) {
                                showToast('Đã xóa từ vựng', 'success');
                                loadVocabularies();
                              } else {
                                showToast('Không thể xóa', 'danger');
                              }
                            } catch (e) {
                              showToast('Có lỗi khi xóa', 'danger');
                            }
                          }}
                        >
                          <i className="bi bi-trash"></i>
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
              {totalPages > 1 && (
                <div className="d-flex justify-content-center mt-3">
                  <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    onPageChange={(p: number) => {
                      setCurrentPage(p);
                      window.scrollTo({ top: 0, behavior: 'smooth' });
                    }}
                  />
                </div>
              )}
            </div>
          )}
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

            {/* Giữ lại duy nhất trường Từ tiếng Anh */}
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
                  Đang xử lý...
                </>
              ) : (
                <>
                  <i className="bi bi-check-circle me-2"></i>
                  Lấy dữ liệu và lưu
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
