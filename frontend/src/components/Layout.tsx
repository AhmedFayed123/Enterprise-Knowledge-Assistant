import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Layout: React.FC<{children: React.ReactNode}> = ({children}) =>{
  const { logout } = useAuth();
  return (
    <div className="min-h-screen flex">
      <aside className="w-64 bg-white border-r p-4 hidden md:block">
        <h2 className="font-bold text-lg mb-4">EKA</h2>
        <nav className="flex flex-col gap-2">
          <Link to="/dashboard" className="text-sm">Dashboard</Link>
          <Link to="/conversations" className="text-sm">Conversations</Link>
          <Link to="/documents" className="text-sm">Documents</Link>
        </nav>
        <div className="mt-6">
          <button onClick={logout} className="text-sm text-red-600">Logout</button>
        </div>
      </aside>
      <main className="flex-1 p-4 bg-gray-50">
        {children}
      </main>
    </div>
  )
}
export default Layout;
