import React, { useState, useEffect } from 'react';
import { Container, Card, Table, Button, Alert, Spinner } from 'react-bootstrap';
import vocabularyService from '../services/vocabularyService';
import { LearnedWord } from '../types';

/**
 * LearnedWords page component
 * Displays all words that the user has saved
 */
const LearnedWordsPage: React.FC = () => {
  const [words, setWords] = useState<LearnedWord[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Fetch all learned words for the current user
   */
  const fetchLearnedWords = async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      const learnedWords = await vocabularyService.getLearnedWords();
      setWords(learnedWords);
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
  const removeWord = async (id: number) => {
    setIsLoading(true);
    
    try {
      await vocabularyService.removeLearnedWord(id);
      // Update the state by removing the word
      setWords(words.filter(word => word.id !== id));
    } catch (err) {
      setError('Không thể xóa từ này. Vui lòng thử lại sau.');
      console.error('Error removing word:', err);
    } finally {
      setIsLoading(false);
    }
  };

  // Load learned words when component mounts
  useEffect(() => {
    fetchLearnedWords();
  }, []);

  return (
    <Container className="py-4">
      <h1 className="mb-4 text-center">Từ vựng đã học</h1>
      
      {error && <Alert variant="danger">{error}</Alert>}
      
      <Card className="shadow-sm">
        <Card.Body>
          {isLoading && words.length === 0 ? (
            <div className="text-center py-5">
              <Spinner animation="border" role="status" variant="primary">
                <span className="visually-hidden">Đang tải...</span>
              </Spinner>
            </div>
          ) : words.length === 0 ? (
            <div className="text-center py-5">
              <p className="text-muted mb-4">Bạn chưa lưu từ vựng nào.</p>
              <Button variant="primary" href="/wordgenerator">
                <i className="bi bi-plus-circle me-2"></i>
                Bắt đầu học từ mới
              </Button>
            </div>
          ) : (
            <div className="table-responsive">
              <Table hover>
                <thead className="table-light">
                  <tr>
                    <th>#</th>
                    <th>Từ vựng</th>
                    <th className="text-end">Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {words.map((word, index) => (
                    <tr key={word.id}>
                      <td>{index + 1}</td>
                      <td>{word.word}</td>
                      <td className="text-end">
                        <Button 
                          variant="outline-danger" 
                          size="sm"
                          onClick={() => removeWord(word.id)}
                          disabled={isLoading}
                        >
                          <i className="bi bi-trash"></i>
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </div>
          )}
        </Card.Body>
      </Card>
    </Container>
  );
};

export default LearnedWordsPage; 