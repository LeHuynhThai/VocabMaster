import React, { useState } from 'react';
import { Button, Form, Spinner, Card } from 'react-bootstrap';
import vocabularyService from '../../services/vocabularyService';
import { TranslationResponse } from '../../types';
import './TranslationBox.css';

interface TranslationBoxProps {
  initialText?: string;
  className?: string;
}

/**
 * TranslationBox component
 * Allows users to translate text from English to Vietnamese
 */
const TranslationBox: React.FC<TranslationBoxProps> = ({ initialText = '', className = '' }) => {
  const [text, setText] = useState(initialText);
  const [translation, setTranslation] = useState<TranslationResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Handles text input change
   */
  const handleTextChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setText(e.target.value);
    // Reset translation when text changes
    if (translation) {
      setTranslation(null);
    }
  };

  /**
   * Handles translation request
   */
  const handleTranslate = async () => {
    if (!text.trim()) {
      setError('Vui lòng nhập văn bản để dịch');
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const result = await vocabularyService.translateEnglishToVietnamese(text);
      setTranslation(result);
    } catch (err) {
      console.error('Translation error:', err);
      setError('Không thể dịch văn bản. Vui lòng thử lại sau.');
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * Clears the form
   */
  const handleClear = () => {
    setText('');
    setTranslation(null);
    setError(null);
  };

  return (
    <Card className={`translation-box ${className}`}>
      <Card.Header>
        <h5 className="mb-0">Dịch văn bản</h5>
      </Card.Header>
      <Card.Body>
        {error && (
          <div className="alert alert-danger" role="alert">
            {error}
          </div>
        )}

        <Form.Group className="mb-3">
          <Form.Label>Văn bản tiếng Anh</Form.Label>
          <Form.Control
            as="textarea"
            rows={3}
            value={text}
            onChange={handleTextChange}
            placeholder="Nhập văn bản tiếng Anh cần dịch..."
            disabled={isLoading}
          />
        </Form.Group>

        <div className="d-flex gap-2 mb-3">
          <Button 
            variant="primary" 
            onClick={handleTranslate} 
            disabled={isLoading || !text.trim()}
          >
            {isLoading ? (
              <>
                <Spinner
                  as="span"
                  animation="border"
                  size="sm"
                  role="status"
                  aria-hidden="true"
                  className="me-2"
                />
                Đang dịch...
              </>
            ) : (
              'Dịch'
            )}
          </Button>
          <Button 
            variant="outline-secondary" 
            onClick={handleClear}
            disabled={isLoading || (!text && !translation)}
          >
            Xóa
          </Button>
        </div>

        {translation && (
          <div className="translation-result">
            <h6>Bản dịch tiếng Việt:</h6>
            <div className="p-3 bg-light rounded">
              {translation.translatedText}
            </div>
          </div>
        )}
      </Card.Body>
    </Card>
  );
};

export default TranslationBox; 