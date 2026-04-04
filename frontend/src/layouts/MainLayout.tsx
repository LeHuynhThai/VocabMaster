import React from 'react';
import Footer from '../components/layout/Footer';
import Header from '../components/layout/Header';
import Sidebar from '../components/layout/Sidebar';

interface MainLayoutProps {
  children: React.ReactNode;
}

const MainLayout: React.FC<MainLayoutProps> = ({ children }) => {
  return (
    <div className="min-h-screen bg-[#f5f7fa] text-slate-900">
      <Header />
      <div className="flex min-h-screen flex-col pt-[70px]">
        <div className="flex flex-1">
          <Sidebar />
          <main className="min-h-[calc(100vh-140px)] flex-1 px-4 py-6 sm:px-6 lg:px-8">
            {children}
          </main>
        </div>
        <Footer />
      </div>
    </div>
  );
};

export default MainLayout;