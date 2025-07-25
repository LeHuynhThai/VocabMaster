import React from 'react';
import { Container } from 'react-bootstrap';
import './Footer.css';

/**
 * Footer component for the application
 * Contains copyright information
 */
const Footer: React.FC = () => {
  // Get current year for copyright
  const currentYear = new Date().getFullYear();
  
  return (
    <footer className="app-footer">
      <Container>
        <div className="text-center py-3">
          <div className="footer-logo">
            <i className="bi bi-book-half me-2"></i>
            VocabMaster
          </div>
          
          <p className="mb-0 mt-2">
            &copy; {currentYear} VocabMaster. All rights reserved.
          </p>
        </div>
      </Container>
    </footer>
  );
};

export default Footer; 