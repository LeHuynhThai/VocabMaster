import React from 'react';
import { Button as BootstrapButton, ButtonProps as BSButtonProps, Spinner } from 'react-bootstrap';

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
  variant,
  ...props
}) => {
  const iconElement = typeof icon === 'string'
    ? <i className={`${icon} text-lg`} />
    : icon;

  const variantClass = (() => {
    const v = (variant as string) || '';
    if (v === 'primary') return 'bg-gradient-to-r from-[#6a11cb] to-[#2575fc] text-white border-none hover:from-[#5a0cb0] hover:to-[#1565e6]';
    if (v === 'outline-primary') return 'border border-[#6a11cb] text-[#6a11cb] hover:bg-[#f5f0ff]';
    return '';
  })();

  const classes = `inline-flex items-center justify-center px-4 py-2 font-medium rounded-md transition-all relative overflow-hidden min-h-[38px] ${variantClass} ${className}`;

  return (
    <BootstrapButton
      variant={variant}
      className={classes}
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
            className="mr-2 w-4 h-4"
          />
          <span className="sr-only">Đang tải...</span>
        </>
      ) : (
        <>
          {icon && iconPosition === 'left' && (
            <span className="mr-2 flex items-center">{iconElement}</span>
          )}
          <span className="button-text">{children}</span>
          {icon && iconPosition === 'right' && (
            <span className="ml-2 flex items-center">{iconElement}</span>
          )}
        </>
      )}
    </BootstrapButton>
  );
};

export default Button; 