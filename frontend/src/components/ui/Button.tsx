import React from 'react';

/**
 * Extended button props
 */
export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  /** Loading state */
  isLoading?: boolean;
  /** Icon component or class name */
  icon?: React.ReactNode | string;
  /** Icon position */
  iconPosition?: 'left' | 'right';
  /** Visual variant */
  variant?: string;
  /** Button size */
  size?: 'sm' | 'lg';
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
  size,
  type,
  ...props
}) => {
  const iconElement = typeof icon === 'string'
    ? <i className={`${icon} text-lg`} />
    : icon;

  const variantClass = (() => {
    const v = (variant as string) || '';
    if (v === 'primary') return 'bg-gradient-to-r from-[#6a11cb] to-[#2575fc] text-white border-none hover:from-[#5a0cb0] hover:to-[#1565e6]';
    if (v === 'outline-primary') return 'border border-[#6a11cb] text-[#6a11cb] hover:bg-[#f5f0ff]';
    if (v === 'outline-danger') return 'border border-rose-300 text-rose-700 hover:bg-rose-50';
    if (v === 'outline-info') return 'border border-sky-300 text-sky-700 hover:bg-sky-50';
    if (v === 'secondary') return 'bg-slate-600 text-white border-none hover:bg-slate-700';
    if (v === 'success') return 'bg-emerald-600 text-white border-none hover:bg-emerald-700';
    return '';
  })();

  const sizeClass = size === 'sm'
    ? 'min-h-9 px-3 py-2 text-sm'
    : size === 'lg'
      ? 'min-h-12 px-5 py-3 text-base'
      : 'min-h-[38px] px-4 py-2 text-sm';

  const classes = [
    'inline-flex items-center justify-center rounded-md font-medium transition-all relative overflow-hidden disabled:cursor-not-allowed disabled:opacity-60',
    sizeClass,
    variantClass,
    className,
  ].join(' ');

  return (
    <button
      type={type ?? 'button'}
      className={classes}
      disabled={isLoading || disabled}
      {...props}
    >
      {isLoading ? (
        <>
          <span className="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent" aria-hidden="true" />
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
    </button>
  );
};

export default Button;