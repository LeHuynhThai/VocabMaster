import React, { useState, useEffect, useCallback } from 'react';
import { Container, Row, Col, Button, Form, InputGroup, Spinner } from 'react-bootstrap';
import { useToast } from '../contexts/ToastContext';
import vocabularyService from '../services/vocabularyService';
import { Vocabulary, Pronunciation, Meaning } from '../types';
import './WordGeneratorPage.css';

// Hàm lấy class CSS dựa trên loại từ
const getPartOfSpeechClass = (partOfSpeech: string): string => {
  const pos = partOfSpeech.toLowerCase();
  
  if (pos.includes('noun')) return 'pos-noun';
  if (pos.includes('verb')) return 'pos-verb';
  if (pos.includes('adjective')) return 'pos-adjective';
  if (pos.includes('adverb')) return 'pos-adverb';
  if (pos.includes('pronoun')) return 'pos-pronoun';
  if (pos.includes('preposition')) return 'pos-preposition';
  if (pos.includes('conjunction')) return 'pos-conjunction';
  if (pos.includes('interjection')) return 'pos-interjection';
  
  return '';
};

const WordGeneratorPage: React.FC = () => {
  const [word, setWord] = useState<Vocabulary | null>(null);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [searching, setSearching] = useState(false);
  const [saving, setSaving] = useState(false);
  const { addToast } = useToast();

  const fetchRandomWord = useCallback(async () => {
    setLoading(true);
    try {
      const data = await vocabularyService.getRandomWord();
      setWord(data);
    } catch (error) {
      console.error('Error fetching random word:', error);
      addToast({
        type: 'error',
        message: 'Không thể lấy từ vựng ngẫu nhiên. Vui lòng thử lại sau.'
      });
    } finally {
      setLoading(false);
    }
  }, [addToast]);

  const fetchNewRandomWord = useCallback(async () => {
    setLoading(true);
    try {
      const data = await vocabularyService.getNewRandomWord();
      setWord(data);
    } catch (error) {
      console.error('Error fetching new random word:', error);
      addToast({
        type: 'error',
        message: 'Không thể lấy từ vựng mới. Vui lòng thử lại sau.'
      });
    } finally {
      setLoading(false);
    }
  }, [addToast]);

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!searchTerm.trim()) return;
    
    setSearching(true);
    try {
      const data = await vocabularyService.lookup(searchTerm.trim());
      setWord(data);
    } catch (error) {
      console.error('Error searching for word:', error);
      addToast({
        type: 'error',
        message: `Không tìm thấy từ "${searchTerm}". Vui lòng kiểm tra lại.`
      });
    } finally {
      setSearching(false);
    }
  };

  const handleSaveWord = async (e?: React.MouseEvent) => {
    if (e) e.preventDefault();
    if (!word || saving) return;
    
    setSaving(true);
    
    try {
      const result = await vocabularyService.addLearnedWord(word.word);
      
      if (result.success) {
        // Cập nhật trạng thái từ hiện tại
        const updatedWord = { ...word, isLearned: true };
        setWord(updatedWord);
        
        // Hiển thị thông báo thành công
        addToast({
          type: 'success',
          message: `Đã lưu từ "${word.word}" vào danh sách từ đã học.`
        });
      } else {
        // Hiển thị thông báo lỗi
        addToast({
          type: 'error',
          message: result.error || 'Không thể lưu từ vựng. Vui lòng thử lại sau.'
        });
      }
    } catch (error) {
      console.error('Error saving word:', error);
      addToast({
        type: 'error',
        message: 'Không thể lưu từ vựng. Vui lòng thử lại sau.'
      });
    } finally {
      setSaving(false);
    }
  };

  const playAudio = (audioUrl: string) => {
    if (!audioUrl) return;
    
    const audio = new Audio(audioUrl);
    audio.play().catch(error => {
      console.error('Error playing audio:', error);
      addToast({
        type: 'error',
        message: 'Không thể phát âm thanh. Vui lòng thử lại sau.'
      });
    });
  };

  useEffect(() => {
    fetchRandomWord();
  }, [fetchRandomWord]);

  return (
    <Container className="word-generator-page">
      <h1 className="page-title">Từ Vựng Mới</h1>
      
      <Form onSubmit={handleSearch} className="search-form mb-4">
        <InputGroup>
          <Form.Control
            type="text"
            placeholder="Nhập từ cần tra cứu..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
          <Button variant="primary" type="submit" disabled={searching}>
            {searching ? <Spinner animation="border" size="sm" /> : 'Tra cứu'}
          </Button>
        </InputGroup>
      </Form>

      {loading ? (
        <div className="text-center my-5">
          <Spinner animation="border" />
          <p className="mt-2">Đang tải từ vựng...</p>
        </div>
      ) : word ? (
        <div className="word-container">
          <div className="word-header">
            <h2 className="word-title">{word.word}</h2>
          </div>

          {word.pronunciations && word.pronunciations.length > 0 && (
            <div className="pronunciation-section">
              <h3 className="pronunciation-title">
                <i className="bi bi-volume-up me-2"></i>
                Phát âm
              </h3>
              <div className="pronunciation-container">
                {word.pronunciations.map((pronunciation: Pronunciation, index: number) => (
                  <div key={index} className="pronunciation-item">
                    {pronunciation.text && (
                      <span className="pronunciation-text">{pronunciation.text}</span>
                    )}
                    {pronunciation.audio && (
                      <Button 
                        variant="link" 
                        className="pronunciation-button"
                        onClick={() => playAudio(pronunciation.audio)}
                      >
                        <i className="bi bi-play-circle-fill"></i>
                      </Button>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}

          {word.meanings && word.meanings.length > 0 && (
            <div className="meanings-container">
              {word.meanings.map((meaning: Meaning, index: number) => (
                <div key={index} className="meaning-item">
                  <div className="part-of-speech-container">
                    <span className={`part-of-speech ${getPartOfSpeechClass(meaning.partOfSpeech)}`}>
                      {meaning.partOfSpeech}
                    </span>
                  </div>
                  
                  {meaning.definitions.map((definition, defIndex) => (
                    <div key={defIndex} className="definition-item">
                      <p className="definition-text">
                        <span className="definition-number">{defIndex + 1}.</span> {definition.text}
                      </p>
                      
                      {definition.example && (
                        <p className="definition-example">
                          <i className="bi bi-quote"></i> {definition.example}
                        </p>
                      )}
                      
                      {definition.synonyms && definition.synonyms.length > 0 && (
                        <div className="synonyms">
                          <span className="synonyms-label">Từ đồng nghĩa:</span>
                          <span className="synonyms-list">{definition.synonyms.join(', ')}</span>
                        </div>
                      )}
                      
                      {definition.antonyms && definition.antonyms.length > 0 && (
                        <div className="antonyms">
                          <span className="antonyms-label">Từ trái nghĩa:</span>
                          <span className="antonyms-list">{definition.antonyms.join(', ')}</span>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              ))}
            </div>
          )}

          <div className="word-actions mt-4">
            <Button 
              variant="primary" 
              onClick={fetchNewRandomWord}
              disabled={loading}
              className="me-2"
            >
              <i className="bi bi-shuffle me-1"></i> Từ vựng mới
            </Button>
            
            <Button 
              variant={word.isLearned ? "success" : "outline-success"} 
              onClick={handleSaveWord}
              disabled={word.isLearned || loading || saving}
            >
              {saving ? (
                <Spinner animation="border" size="sm" className="me-1" />
              ) : (
                <i className={`bi ${word.isLearned ? "bi-check-circle-fill" : "bi-plus-circle"} me-1`}></i>
              )}
              {word.isLearned ? 'Đã lưu' : saving ? 'Đang lưu...' : 'Lưu từ này'}
            </Button>
          </div>
        </div>
      ) : (
        <div className="text-center my-5">
          <p>Không tìm thấy từ vựng. Vui lòng thử lại.</p>
          <Button variant="primary" onClick={fetchRandomWord}>
            Tải từ vựng ngẫu nhiên
          </Button>
        </div>
      )}
    </Container>
  );
};

export default WordGeneratorPage; 