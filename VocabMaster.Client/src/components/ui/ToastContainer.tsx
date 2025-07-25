import React, { useState, useEffect } from 'react';
import { createPortal } from 'react-dom';
import './ToastContainer.css';

/**
 * ToastContainer component props
 */
interface ToastContainerProps {
  /** Child elements */
  children: React.ReactNode;
}

/**
 * ToastContainer component
 * Container for toast notifications that positions them at the top of the screen
 */
const ToastContainer: React.FC<ToastContainerProps> = ({ children }) => {
  const [container] = useState(() => {
    // Create toast container element if it doesn't exist
    const existingContainer = document.getElementById('toast-container');
    if (existingContainer) {
      return existingContainer;
    }

    const newContainer = document.createElement('div');
    newContainer.id = 'toast-container';
    newContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
    document.body.appendChild(newContainer);
    return newContainer;
  });

  // Clean up container when component unmounts
  useEffect(() => {
    return () => {
      if (container && container.childNodes.length === 0) {
        document.body.removeChild(container);
      }
    };
  }, [container]);

  return createPortal(children, container);
};

export default ToastContainer; 