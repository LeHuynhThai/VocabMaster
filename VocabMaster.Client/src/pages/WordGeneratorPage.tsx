import React, { useState } from 'react';
import { Container, Card, Button, Spinner, ListGroup, Row, Col, Form, InputGroup } from 'react-bootstrap';
import vocabularyService from '../services/vocabularyService';
import { Vocabulary, DictionaryResponse } from '../types';
import Toast, { ToastType } from '../components/Toast';
import './WordGeneratorPage.css';

/**
 * WordGenerator page component
 * Allows users to get random words and look up their definitions
 */
const WordGeneratorPage: React.FC = () => {
  const [currentWord, setCurrentWord] = useState<Vocabulary | null>(null);
  const [wordDetails, setWordDetails] = useState<DictionaryResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  
  // Toast notification state
  const [toast, setToast] = useState({
    show: false,
    type: 'success' as ToastType,
    message: ''
  });

  /**
   * Fetch a random word from the API
   */
  const fetchRandomWord = async () => {
    setIsLoading(true);
    setError(null);
    setSearchTerm('');
    
    try {
      const response = await vocabularyService.getRandomWord();
      setCurrentWord(response);
      
      // If response already contains dictionary information, use it directly
      if (response.phonetic && response.meanings) {
        setWordDetails({
          word: response.word,
          phonetic: response.phonetic,
          phonetics: response.phonetics || [],
          meanings: response.meanings || []
        });
      } else {
        // Otherwise, look up the word definition
        await lookupWord(response.word);
      }
    } catch (err) {
      setError('Không thể tải từ ngẫu nhiên. Vui lòng thử lại sau.');
      console.error('Error fetching random word:', err);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Look up the definition of a word
   * @param word - The word to look up
   */
  const lookupWord = async (word: string) => {
    setIsLoading(true);
    setError(null);
    
    try {
      const details = await vocabularyService.lookupWord(word);
      setWordDetails(details);
      setCurrentWord({ id: 0, word: details.word });
    } catch (err) {
      setError('Không thể tìm định nghĩa cho từ này.');
      console.error('Error looking up word:', err);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handle search form submission
   */
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchTerm.trim()) {
      lookupWord(searchTerm.trim());
    }
  };

  /**
   * Save current word to user's learned words
   */
  const saveWord = async () => {
    if (!currentWord) return;
    
    setIsLoading(true);
    try {
      await vocabularyService.addLearnedWord(currentWord.word);
      
      // Show success toast notification
      setToast({
        show: true,
        type: 'success',
        message: `Từ "${currentWord.word}" đã được lưu vào danh sách từ đã học.`
      });
      
    } catch (err) {
      setError('Không thể lưu từ này. Vui lòng thử lại sau.');
      console.error('Error saving word:', err);
      
      // Show error toast notification
      setToast({
        show: true,
        type: 'danger',
        message: 'Không thể lưu từ này. Vui lòng thử lại sau.'
      });
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Handle toast close event
   */
  const handleToastClose = () => {
    setToast(prev => ({ ...prev, show: false }));
  };

  return (
    <Container className="py-4">
      {/* Page title */}
      <div className="page-header">
        <h1 className="page-title">Từ vựng mới</h1>
        <p className="page-description">
          Học từ mới mỗi ngày để cải thiện vốn từ vựng của bạn
        </p>
      </div>
      
      {/* Search form */}
      <Card className="search-card mb-4">
        <Card.Body>
          <Form onSubmit={handleSearch}>
            <Form.Group>
              <Form.Label>Tra từ điển</Form.Label>
              <InputGroup>
                <Form.Control
                  type="text"
                  placeholder="Nhập từ cần tra cứu..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
                <Button 
                  variant="primary" 
                  type="submit"
                  disabled={isLoading || !searchTerm.trim()}
                >
                  <i className="bi bi-search me-1"></i> Tra cứu
                </Button>
              </InputGroup>
            </Form.Group>
          </Form>
        </Card.Body>
      </Card>
      
      {/* Word card */}
      <Card className="word-card mb-4">
        <Card.Body>
          {isLoading ? (
            <div className="text-center py-5">
              <Spinner animation="border" role="status" variant="primary">
                <span className="visually-hidden">Đang tải...</span>
              </Spinner>
            </div>
          ) : error ? (
            <div className="text-center py-4">
              <p className="text-danger">{error}</p>
              <Button variant="primary" onClick={fetchRandomWord} className="action-button-primary">
                Thử lại
              </Button>
            </div>
          ) : !currentWord ? (
            <div className="text-center py-5">
              <p className="mb-4">Nhấn nút bên dưới để bắt đầu học từ mới</p>
              <Button 
                variant="primary" 
                onClick={fetchRandomWord}
                className="action-button-primary"
                size="lg"
              >
                <i className="bi bi-play-circle me-2"></i>
                Bắt đầu
              </Button>
            </div>
          ) : (
            <div>
              {/* Word header section */}
              <div className="word-header text-center mb-4">
                <h2 className="word-title">{currentWord.word}</h2>
                {wordDetails?.phonetic && (
                  <p className="word-phonetic">{wordDetails.phonetic}</p>
                )}
                
                {/* Audio pronunciation */}
                {wordDetails?.phonetics && wordDetails.phonetics.some(p => p.audio) && (
                  <div className="audio-container">
                    <audio controls className="audio-player">
                      <source 
                        src={wordDetails.phonetics.find(p => p.audio)?.audio} 
                        type="audio/mpeg" 
                      />
                      Trình duyệt của bạn không hỗ trợ phát âm thanh.
                    </audio>
                  </div>
                )}
              </div>
              
              {/* Word definitions */}
              {wordDetails && wordDetails.meanings && (
                <div className="word-meanings">
                  {wordDetails.meanings.map((meaning, index) => (
                    <div key={index} className="meaning-section">
                      <h5 className="part-of-speech">
                        <span className="badge">{meaning.partOfSpeech}</span>
                      </h5>
                      
                      <ListGroup variant="flush" className="definitions-list">
                        {meaning.definitions.map((def, defIndex) => (
                          <ListGroup.Item key={defIndex} className="definition-item">
                            <div className="definition-text">{def.text}</div>
                            
                            {def.example && (
                              <div className="definition-example">
                                "{def.example}"
                              </div>
                            )}
                            
                            {/* Synonyms */}
                            {def.synonyms && def.synonyms.length > 0 && (
                              <div className="definition-synonyms">
                                <strong>Từ đồng nghĩa:</strong> {def.synonyms.join(', ')}
                              </div>
                            )}
                            
                            {/* Antonyms */}
                            {def.antonyms && def.antonyms.length > 0 && (
                              <div className="definition-antonyms">
                                <strong>Từ trái nghĩa:</strong> {def.antonyms.join(', ')}
                              </div>
                            )}
                          </ListGroup.Item>
                        ))}
                      </ListGroup>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
        </Card.Body>
        
        {/* Card footer with actions */}
        {currentWord && (
          <Card.Footer className="word-actions">
            <Row>
              <Col xs={12} md={6} className="mb-2 mb-md-0">
                <Button 
                  variant="outline-primary" 
                  className="action-button"
                  onClick={fetchRandomWord}
                  disabled={isLoading}
                >
                  <i className="bi bi-arrow-repeat me-2"></i>
                  Từ khác
                </Button>
              </Col>
              
              <Col xs={12} md={6}>
                <Button 
                  variant="primary"
                  className="action-button"
                  onClick={saveWord}
                  disabled={isLoading || !currentWord}
                >
                  <i className="bi bi-bookmark-plus me-2"></i>
                  Lưu từ này
                </Button>
              </Col>
            </Row>
          </Card.Footer>
        )}
      </Card>
      
      {/* Toast notification */}
      <Toast 
        show={toast.show}
        type={toast.type}
        message={toast.message}
        onClose={handleToastClose}
      />
    </Container>
  );
};

export default WordGeneratorPage; 