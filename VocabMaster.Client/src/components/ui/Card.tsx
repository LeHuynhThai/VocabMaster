import React from 'react';
import { Card as BootstrapCard } from 'react-bootstrap';
import './Card.css';

/**
 * Card component props
 */
export interface CardProps {
  /** Card title */
  title?: React.ReactNode;
  /** Card subtitle */
  subtitle?: React.ReactNode;
  /** Card content */
  children: React.ReactNode;
  /** Footer content */
  footer?: React.ReactNode;
  /** Additional class name */
  className?: string;
  /** Card header icon */
  icon?: React.ReactNode | string;
  /** Whether to show a shadow effect */
  shadow?: boolean;
  /** Whether to show a hover effect */
  hover?: boolean;
}

/**
 * Custom Card component with consistent styling
 */
const Card: React.FC<CardProps> = ({
  title,
  subtitle,
  children,
  footer,
  className = '',
  icon,
  shadow = true,
  hover = false,
}) => {
  // Determine if the icon is a string (class name) or a component
  const iconElement = typeof icon === 'string' 
    ? <i className={`${icon} card-icon`}></i> 
    : icon;

  // Build class name
  const cardClassName = `
    custom-card 
    ${shadow ? 'with-shadow' : ''} 
    ${hover ? 'with-hover' : ''} 
    ${className}
  `;

  return (
    <BootstrapCard className={cardClassName}>
      {(title || subtitle) && (
        <BootstrapCard.Header className="card-custom-header">
          {icon && <div className="card-icon-container">{iconElement}</div>}
          <div className="card-header-content">
            {title && <BootstrapCard.Title>{title}</BootstrapCard.Title>}
            {subtitle && <BootstrapCard.Subtitle>{subtitle}</BootstrapCard.Subtitle>}
          </div>
        </BootstrapCard.Header>
      )}
      
      <BootstrapCard.Body>
        {children}
      </BootstrapCard.Body>
      
      {footer && (
        <BootstrapCard.Footer className="card-custom-footer">
          {footer}
        </BootstrapCard.Footer>
      )}
    </BootstrapCard>
  );
};

export default Card; 