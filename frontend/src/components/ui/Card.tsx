import React from 'react';

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
  const iconElement = typeof icon === 'string'
    ? <i className={`${icon} text-2xl text-[#6a11cb]`}></i>
    : icon;

  const cardClassName = [
    'overflow-hidden rounded-xl border border-slate-100 bg-white transition-all duration-300',
    shadow ? 'shadow-[0_4px_20px_rgba(0,0,0,0.08)]' : '',
    hover ? 'hover:-translate-y-1 hover:shadow-[0_10px_25px_rgba(0,0,0,0.12)]' : '',
    className,
  ].join(' ');

  return (
    <section className={cardClassName}>
      {(title || subtitle) && (
        <div className="flex items-center border-b border-black/5 bg-white px-6 py-5">
          {icon && <div className="mr-4 flex items-center justify-center">{iconElement}</div>}
          <div className="flex-1">
            {title && <h3 className="mb-1 text-xl font-semibold text-slate-800">{title}</h3>}
            {subtitle && <p className="m-0 text-sm text-slate-500">{subtitle}</p>}
          </div>
        </div>
      )}

      <div className="p-6">
        {children}
      </div>

      {footer && (
        <div className="border-t border-black/5 bg-slate-50 px-6 py-4">
          {footer}
        </div>
      )}
    </section>
  );
};

export default Card; 