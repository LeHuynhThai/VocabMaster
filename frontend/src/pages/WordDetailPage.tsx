import React, { useEffect, useState } from 'react';
import { Container, Card, Button, Spinner, Alert } from 'react-bootstrap';
import { useParams, useNavigate } from 'react-router-dom';
import { Vocabulary, Pronunciation, Meaning } from '../types';
import api from '../services/api';
import './WordGeneratorPage.css';

const WordDetailPage: React.FC = () => {
  const { word } = useParams<{ word: string }>();
  const navigate = useNavigate();
  const [vocabulary, setVocabulary] = useState<Vocabulary | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (word) {
      fetchWordDetail(word);
    }
  }, [word]);

  const fetchWordDetail = async (w: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await api.get(`/api/wordgenerator/word-detail/${encodeURIComponent(w)}`);
      setVocabulary(response.data);
    } catch (err) {
      setError('Không thể tải thông tin từ vựng.');
      console.error('Error fetching word detail:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const playAudio = (audioUrl: string) => {
    if (!audioUrl) return;
    try {
      const audio = new Audio(audioUrl);
      audio.play().catch(err => console.error('Error playing audio:', err));
    } catch (err) {
      console.error('Error creating audio element:', err);
    }
  };

  if (isLoading) {
    return (
      <Container className="py-4">
        <div className="text-center">
          <Spinner animation="border" role="status" variant="primary">
            <span className="visually-hidden">Đang tải...</span>
          </Spinner>
        </div>
      </Container>
    );
  }

  if (error) {
    return (
      <Container className="py-4">
        <Alert variant="danger">
          <Alert.Heading>Lỗi</Alert.Heading>
          <p>{error}</p>
          <Button variant="outline-danger" onClick={() => navigate(-1)}>Quay lại</Button>
        </Alert>
      </Container>
    );
  }

  if (!vocabulary) {
    return (
      <Container className="py-4">
        <Alert variant="warning">
          <Alert.Heading>Không tìm thấy</Alert.Heading>
          <p>Không tìm thấy từ vựng "{word}"</p>
          <Button variant="outline-warning" onClick={() => navigate(-1)}>Quay lại</Button>
        </Alert>
      </Container>
    );
  }

  return (
    <Container className="py-4 word-generator-page">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <Button variant="outline-secondary" onClick={() => navigate(-1)}>
          <i className="bi bi-arrow-left me-2"></i>
          Quay lại
        </Button>
      </div>

      <div className="word-container">
        <div className="word-header">
          <h2 className="word-title">{vocabulary.word}</h2>

          {vocabulary.vietnamese && (
            <div className="vietnamese-translation">
              <h3 className="vietnamese-title">Nghĩa tiếng Việt:</h3>
              <p className="vietnamese-text">{vocabulary.vietnamese}</p>
            </div>
          )}

          {!vocabulary.vietnamese && (
            <div className="vietnamese-translation vietnamese-missing">
              <h3 className="vietnamese-title">Nghĩa tiếng Việt:</h3>
              <p className="vietnamese-text">Chưa có bản dịch</p>
            </div>
          )}

          {vocabulary.pronunciations && vocabulary.pronunciations.length > 0 && (
            <div className="pronunciation-container">
              {vocabulary.pronunciations.map((pronunciation: Pronunciation, index: number) => (
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

        {vocabulary.meanings && vocabulary.meanings.length > 0 && (
          <div className="meanings-container">
            {vocabulary.meanings.map((meaning: Meaning, index: number) => (
              <div key={index} className="meaning-item">
                <div className="part-of-speech-container">
                  <span className={`part-of-speech`}>
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
      </div>
    </Container>
  );
};

export default WordDetailPage;


