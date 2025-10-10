import React from 'react';
import { Button as BootstrapButton, ButtonProps as BSButtonProps, Spinner } from 'react-bootstrap';
import './Button.css';

/**
 * Extended button props
 */
export interface ButtonProps extends BSButtonProps {
  /** Loading state */
  isLoading?: boolean;
  /** Icon component or class name */
  icon?: React.ReactNode | string;
  /** Icon position */
  iconPosition?: 'left' | 'right';
}

/**
 * Custom button component with loading state and icon support
 */
const Button: React.FC<ButtonProps> = ({
  children,
  isLoading = false,
  icon,
  iconPosition = 'left',
  className = '',
  disabled,
  ...props
}) => {
  // Determine if the icon is a string (class name) or a component
  const iconElement = typeof icon === 'string' 
    ? <i className={`${icon} button-icon`}></i> 
    : icon;

  return (
    <BootstrapButton
      className={`custom-button ${className}`}
      disabled={isLoading || disabled}
      {...props}
    >
      {isLoading ? (
        <>
          <Spinner
            as="span"
            animation="border"
            size="sm"
            role="status"
            aria-hidden="true"
            className="button-spinner"
          />
          <span className="visually-hidden">Đang tải...</span>
        </>
      ) : (
        <>
          {icon && iconPosition === 'left' && (
            <span className="icon-left">{iconElement}</span>
          )}
          <span className="button-text">{children}</span>
          {icon && iconPosition === 'right' && (
            <span className="icon-right">{iconElement}</span>
          )}
        </>
      )}
    </BootstrapButton>
  );
};

export default Button; 