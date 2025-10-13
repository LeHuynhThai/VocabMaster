import React, { useState, useEffect, useCallback } from 'react';
import { Container, Row, Col, Button, Form, InputGroup, Spinner } from 'react-bootstrap';
import { useToast } from '../contexts/ToastContext';
import { useNavigate, useLocation } from 'react-router-dom';
import { Vocabulary, Pronunciation, Meaning } from '../types';
import vocabularyService from '../services/vocabularyService';
import './WordGeneratorPage.css';
import { ROUTES } from '../utils/constants';

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
  const location = useLocation();
  const navigate = useNavigate();
  const [allLearned, setAllLearned] = useState(false);
  const [allLearnedMessage, setAllLearnedMessage] = useState('');

  // Lấy từ vựng từ URL query parameter
  const getWordFromUrl = useCallback(() => {
    const queryParams = new URLSearchParams(location.search);
    const wordParam = queryParams.get('word');
    return wordParam;
  }, [location.search]);

  const fetchRandomWord = useCallback(async () => {
    setLoading(true);
    try {
      const data = await vocabularyService.getRandomWord();
      console.log('Random Word Data:', data);
      
      // Kiểm tra nếu tất cả từ vựng đã được học hết
      if (data.allLearned) {
        setAllLearned(true);
        setAllLearnedMessage(data.message || 'Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống.');
        setWord(null);
      } else {
        console.log('Vietnamese translation:', data.vietnamese);
        setWord(data);
        setAllLearned(false);
        setAllLearnedMessage('');
      }
      
      // Xóa tham số từ URL khi hiển thị từ ngẫu nhiên
      navigate(ROUTES.WORD_GENERATOR, { replace: true });
    } catch (error) {
      console.error('Error fetching random word:', error);
      addToast({
        type: 'error',
        message: 'Bạn đã học hết tất cả từ vựng trong hệ thống.'
      });
    } finally {
      setLoading(false);
    }
  }, [addToast, navigate]);

  const fetchNewRandomWord = useCallback(async () => {
    setLoading(true);
    try {
      const data = await vocabularyService.getNewRandomWord();
      console.log('New Random Word Data:', data);
      
      // Kiểm tra nếu tất cả từ vựng đã được học hết
      if (data.allLearned) {
        setAllLearned(true);
        setAllLearnedMessage(data.message || 'Chúc mừng! Bạn đã học hết tất cả từ vựng trong hệ thống.');
        setWord(null);
      } else {
        console.log('Vietnamese translation:', data.vietnamese);
        setWord(data);
        setAllLearned(false);
        setAllLearnedMessage('');
      }
      
      // Xóa tham số từ URL khi hiển thị từ ngẫu nhiên mới
      navigate(ROUTES.WORD_GENERATOR, { replace: true });
    } catch (error) {
      console.error('Error fetching new random word:', error);
      addToast({
        type: 'error',
        message: 'Bạn đã học hết tất cả từ vựng trong hệ thống.'
      });
    } finally {
      setLoading(false);
    }
  }, [addToast, navigate]);


  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!searchTerm.trim()) return;
    
    setSearching(true);
    try {
      // Search functionality removed - lookup method no longer available
      addToast({
        type: 'info',
        message: 'Chức năng tìm kiếm từ vựng tạm thời không khả dụng'
      });
    } finally {
      setSearching(false);
    }
  };

  const handleSaveWord = async (e?: React.MouseEvent) => {
    if (e) e.preventDefault();
    if (!word || word.isLearned || saving) return;
    
    setSaving(true);
    try {
      const success = await vocabularyService.markAsLearned(word.word);
      
      if (success) {
        const updatedWord = { ...word, isLearned: true };
        setWord(updatedWord);
        
        addToast({
          type: 'success',
          message: `Đã lưu từ "${word.word}" vào danh sách từ vựng đã học.`
        });
      } else {
        addToast({
          type: 'error',
          message: 'Không thể lưu từ vựng. Vui lòng thử lại sau.'
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
    
    try {
      const audio = new Audio(audioUrl);
      audio.play().catch(error => {
        console.error('Error playing audio:', error);
      });
    } catch (error) {
      console.error('Error creating audio element:', error);
    }
  };

  useEffect(() => {
    const wordFromUrl = getWordFromUrl();
    
    if (wordFromUrl) {
      // URL word lookup functionality removed - lookup method no longer available
      addToast({
        type: 'info',
        message: 'Chức năng tìm kiếm từ URL tạm thời không khả dụng'
      });
      // Clear URL parameter and fetch random word instead
      navigate(ROUTES.WORD_GENERATOR, { replace: true });
      fetchRandomWord();
    } else {
      fetchRandomWord();
    }
  }, [getWordFromUrl, fetchRandomWord, addToast, navigate]);

  return (
    <Container className="py-4">
      <Row>
        <Col lg={8} className="mx-auto">
          <Form onSubmit={handleSearch} className="mb-4">
            <InputGroup>
              <Form.Control
                type="text"
                placeholder="Nhập từ bạn muốn tra cứu..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                disabled={searching || loading}
              />
              <Button 
                type="submit" 
                variant="outline-primary"
                disabled={searching || loading || !searchTerm.trim()}
              >
                {searching ? (
                  <Spinner animation="border" size="sm" />
                ) : (
                  <i className="bi bi-search"></i>
                )}
              </Button>
            </InputGroup>
          </Form>
        </Col>
      </Row>

      {loading ? (
        <div className="text-center my-5">
          <Spinner animation="border" role="status" variant="primary">
            <span className="visually-hidden">Đang tải...</span>
          </Spinner>
          <p className="mt-3">Đang tải từ vựng...</p>
        </div>
      ) : allLearned ? (
        <div className="text-center my-5 all-learned-container">
          <div className="all-learned-message">
            <i className="bi bi-trophy-fill text-warning mb-3" style={{ fontSize: '3rem' }}></i>
            <h3 className="mb-4">{allLearnedMessage}</h3>
            <p>Bạn đã hoàn thành việc học tất cả từ vựng có trong hệ thống.</p>
            <p>Hãy tiếp tục ôn tập các từ đã học để củng cố kiến thức!</p>
            <Button 
              variant="primary" 
              className="mt-3"
              onClick={() => navigate(ROUTES.LEARNED_WORDS)}
            >
              <i className="bi bi-journal-text me-2"></i>
              Xem danh sách từ đã học
            </Button>
          </div>
        </div>
      ) : word ? (
        <div className="word-container">
          <div className="word-header">
            <h2 className="word-title">{word.word}</h2>
            
            {word.vietnamese && (
              <div className="vietnamese-translation">
                <h3 className="vietnamese-title">Nghĩa tiếng Việt:</h3>
                <p className="vietnamese-text">{word.vietnamese}</p>
              </div>
            )}
            
            {!word.vietnamese && (
              <div className="vietnamese-translation vietnamese-missing">
                <h3 className="vietnamese-title">Nghĩa tiếng Việt:</h3>
                <p className="vietnamese-text">Chưa có bản dịch</p>
              </div>
            )}

            {word.pronunciations && word.pronunciations.length > 0 && (
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
            )}
            </div>

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