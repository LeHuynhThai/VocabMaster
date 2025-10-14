import React, { useState, useEffect } from 'react';
import { Container, Card, Table, Button, Alert, Spinner, Form, InputGroup } from 'react-bootstrap';
import vocabularyService, { LearnedWord } from '../services/vocabularyService';
import { useAuth } from '../contexts/AuthContext';
import Pagination from '../components/ui/Pagination';
import useToast from '../hooks/useToast';
import './LearnedWordsPage.css';

/**
 * Format date to local format
 */
const formatDate = (dateString?: string): string => {
  if (!dateString) return '';
  
  try {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    }).format(date);
  } catch (error) {
    return '';
  }
};

/**
 * LearnedWords page component
 * Displays all words that the user has saved with pagination
 */
const LearnedWordsPage: React.FC = () => {
  const [words, setWords] = useState<LearnedWord[]>([]);
  const [filteredWords, setFilteredWords] = useState<LearnedWord[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10); // Fixed page size of 10
  const [totalItems, setTotalItems] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const { isAuthenticated } = useAuth();
  const { showToast } = useToast();

  /**
   * Fetch paginated learned words for the current user
   */
  const fetchLearnedWords = async (page: number = currentPage) => {
    if (!isAuthenticated) {
      return; // do not call API if not authenticated
    }
    
    setIsLoading(true);
    setError(null);
    
    try {
      // Use non-paginated endpoint and compute pagination client-side
      const all = await vocabularyService.getLearnedWords();
      const totalItemsLocal = all.length;
      const totalPagesLocal = Math.max(1, Math.ceil(totalItemsLocal / pageSize));
      const start = (page - 1) * pageSize;
      const items = all.slice(start, start + pageSize);

      setWords(items);
      setFilteredWords(items);
      setTotalItems(totalItemsLocal);
      setTotalPages(totalPagesLocal);
      setCurrentPage(page);
    } catch (err) {
      setError('Không thể tải danh sách các từ đã học. Vui lòng thử lại sau.');
      console.error('Error fetching learned words:', err);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Remove a word from the learned words list
   */
  const removeWord = async (id: number, word: string) => {
    // Show confirmation dialog
    if (!window.confirm(`Bạn có chắc chắn muốn xóa từ "${word}" khỏi danh sách đã học?`)) {
      return;
    }

    setIsLoading(true);
    
    try {
      const success = await vocabularyService.removeLearnedWord(id);
      if (success) {
        showToast('Đã xóa từ vựng thành công', 'success');
        // Reload the current page to reflect changes
        fetchLearnedWords(currentPage);
      } else {
        showToast('Không thể xóa từ này. Vui lòng thử lại sau.', 'danger');
      }
    } catch (err) {
      showToast('Không thể xóa từ này. Vui lòng thử lại sau.', 'danger');
      console.error('Error removing word:', err);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handle search input change
   */
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const query = e.target.value;
    setSearchQuery(query);
    
    if (!query.trim()) {
      setFilteredWords(words);
      setTotalItems(words.length);
      setTotalPages(Math.max(1, Math.ceil(words.length / pageSize)));
    } else {
      const filtered = words.filter(word => 
        word.word.toLowerCase().includes(query.toLowerCase())
      );
      setFilteredWords(filtered);
      setTotalItems(filtered.length);
      setTotalPages(Math.max(1, Math.ceil(filtered.length / pageSize)));
    }
    
    // Reset to page 1 when searching
    setCurrentPage(1);
  };

  /**
   * Handle page change
   */
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    
    // If searching, paginate filtered results
    if (searchQuery.trim()) {
      const start = (page - 1) * pageSize;
      const end = start + pageSize;
      const paginatedFiltered = words
        .filter(word => word.word.toLowerCase().includes(searchQuery.toLowerCase()))
        .slice(start, end);
      setFilteredWords(paginatedFiltered);
    } else {
      // If not searching, fetch from server
      fetchLearnedWords(page);
    }
    
    // Scroll to top when changing page
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // load learned words when component mounts or authentication changes
  useEffect(() => {
    if (isAuthenticated) {
      fetchLearnedWords(1);
    }
  }, [isAuthenticated]);

  // Reload data when page becomes visible
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible' && isAuthenticated) {
        fetchLearnedWords(currentPage);
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [isAuthenticated, currentPage]);

  // retry loading if there was an error
  const handleRetry = () => {
    fetchLearnedWords(currentPage);
  };

  return (
    <Container className="py-4 learned-words-page">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <Button 
          variant="outline-primary" 
          onClick={() => fetchLearnedWords(currentPage)} 
          disabled={isLoading}
        >
          <i className="bi bi-arrow-clockwise me-2"></i>
          Làm mới
        </Button>
        
        <div className="search-container" style={{ width: '40%' }}>
          <InputGroup>
            <Form.Control
              type="text"
              placeholder="Tìm kiếm từ vựng đã học..."
              value={searchQuery}
              onChange={handleSearchChange}
              disabled={isLoading}
            />
            <InputGroup.Text>
              <i className="bi bi-search"></i>
            </InputGroup.Text>
          </InputGroup>
        </div>
      </div>
      
      {error && (
        <Alert variant="danger" className="d-flex justify-content-between align-items-center">
          <div>{error}</div>
          <Button variant="outline-danger" size="sm" onClick={handleRetry}>
            Thử lại
          </Button>
        </Alert>
      )}
      
      <Card>
        <Card.Body>
          {isLoading ? (
            <div className="text-center py-5">
              <Spinner animation="border" role="status" variant="primary">
                <span className="visually-hidden">Đang tải...</span>
              </Spinner>
            </div>
          ) : filteredWords.length === 0 ? (
            <div className="text-center py-5">
              {searchQuery.trim() ? (
                <p className="text-muted mb-4">Không tìm thấy từ vựng nào phù hợp với "{searchQuery}".</p>
              ) : (
                <p className="text-muted mb-4">Bạn chưa lưu từ vựng nào.</p>
              )}
            </div>
          ) : (
            <>
              <div className="table-responsive">
                <Table hover>
                  <thead className="table-light">
                    <tr>
                      <th>#</th>
                      <th>Từ vựng</th>
                      <th>Ngày học</th>
                      <th className="text-end">Thao tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredWords.map((word, index) => (
                      <tr key={word.id || index}>
                        <td>{((currentPage - 1) * pageSize) + index + 1}</td>
                        <td>{word.word}</td>
                        <td>{word.learnedAt ? formatDate(word.learnedAt) : '-'}</td>
                        <td className="text-end">
                          <Button 
                            variant="outline-danger" 
                            size="sm"
                            onClick={() => removeWord(word.id, word.word)}
                            disabled={isLoading}
                            title="Xóa từ vựng"
                          >
                            <i className="bi bi-trash"></i>
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
              
              {/* Pagination */}
              <Pagination 
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={handlePageChange}
              />
            </>
          )}
        </Card.Body>
      </Card>
    </Container>
  );
};

export default LearnedWordsPage; 