import React from 'react';

interface AuthLayoutProps {
  title: string;
  subtitle: string;
  children: React.ReactNode;
  footer: React.ReactNode;
}

const AuthLayout: React.FC<AuthLayoutProps> = ({ title, subtitle, children, footer }) => {
  return (
    <div className="flex min-h-[calc(100vh-140px)] items-center justify-center px-4 py-8 sm:px-6">
      <div className="w-full max-w-md rounded-[1.75rem] bg-white p-6 shadow-xl ring-1 ring-slate-200 sm:p-8">
        <div className="mb-6 text-center">
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-gradient-soft text-2xl text-brand-primary">
            <i className="bi bi-book-half"></i>
          </div>
          <h1 className="mb-2 text-3xl font-bold text-slate-900">{title}</h1>
          <p className="m-0 text-sm text-slate-500">{subtitle}</p>
        </div>

        {children}

        <div className="mt-8 text-center text-sm text-slate-600">{footer}</div>
      </div>
    </div>
  );
};

export default AuthLayout;