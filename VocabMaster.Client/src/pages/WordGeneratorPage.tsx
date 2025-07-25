import React, { useState, useEffect } from 'react';
import { Container, Card, Button, Spinner, Form, ListGroup } from 'react-bootstrap';
import vocabularyService from '../services/vocabularyService';
import { Vocabulary, DictionaryResponse } from '../types';

/**
 * WordGenerator page component
 * Allows users to get random words and look up their definitions
 */
const WordGeneratorPage: React.FC = () => {
  const [currentWord, setCurrentWord] = useState<Vocabulary | null>(null);
  const [wordDetails, setWordDetails] = useState<DictionaryResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Fetch a random word from the API
   */
  const fetchRandomWord = async () => {
    setIsLoading(true);
    setError(null);
    setWordDetails(null);
    
    try {
      const word = await vocabularyService.getRandomWord();
      setCurrentWord(word);
      // Automatically look up the word definition
      await lookupWord(word.word);
    } catch (err) {
      setError('Không thể tải từ ngẫu nhiên. Vui lòng thử lại sau.');
      console.error('Error fetching random word:', err);
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Look up the definition of a word
   */
  const lookupWord = async (word: string) => {
    setIsLoading(true);
    setError(null);
    
    try {
      const details = await vocabularyService.lookupWord(word);
      setWordDetails(details);
    } catch (err) {
      setError('Không thể tìm định nghĩa cho từ này.');
      console.error('Error looking up word:', err);
    } finally {
      setIsLoading(false);
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
      // Success notification could be added here
    } catch (err) {
      setError('Không thể lưu từ này. Vui lòng thử lại sau.');
      console.error('Error saving word:', err);
    } finally {
      setIsLoading(false);
    }
  };

  // Load initial word when component mounts
  useEffect(() => {
    fetchRandomWord();
  }, []);

  return (
    <Container className="py-4">
      <h1 className="mb-4 text-center">Từ vựng mới</h1>
      
      <Card className="mb-4 shadow-sm">
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
              <Button variant="primary" onClick={fetchRandomWord}>
                Thử lại
              </Button>
            </div>
          ) : (
            <div>
              {currentWord && (
                <div className="text-center mb-4">
                  <h2 className="display-4 fw-bold">{currentWord.word}</h2>
                  {wordDetails?.phonetic && (
                    <p className="text-muted fs-5">{wordDetails.phonetic}</p>
                  )}
                </div>
              )}
              
              {/* Word definitions */}
              {wordDetails && wordDetails.meanings && (
                <div className="mt-4">
                  {wordDetails.meanings.map((meaning, index) => (
                    <div key={index} className="mb-4">
                      <h5 className="fw-bold">
                        <span className="badge bg-light text-dark me-2">
                          {meaning.partOfSpeech}
                        </span>
                      </h5>
                      <ListGroup variant="flush">
                        {meaning.definitions.map((def, defIndex) => (
                          <ListGroup.Item key={defIndex}>
                            <div className="fw-medium">{def.text}</div>
                            {def.example && (
                              <div className="text-muted fst-italic mt-1">
                                "{def.example}"
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
        <Card.Footer className="bg-white border-0 text-center py-3">
          <Button 
            variant="primary" 
            className="me-2"
            onClick={fetchRandomWord}
            disabled={isLoading}
          >
            <i className="bi bi-arrow-repeat me-2"></i>
            Từ khác
          </Button>
          <Button 
            variant="success"
            onClick={saveWord}
            disabled={isLoading || !currentWord}
          >
            <i className="bi bi-bookmark-plus me-2"></i>
            Lưu từ này
          </Button>
        </Card.Footer>
      </Card>
    </Container>
  );
};

export default WordGeneratorPage; 