import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import LoginPage from './pages/Login';
import RegisterPage from './pages/Register';
import DashboardPage from './pages/Dashboard';
import ConversationsPage from './pages/Conversations';
import ChatPage from './pages/Chat';
import DocumentsPage from './pages/Documents';
import ProtectedRoute from './components/ProtectedRoute';

export default function App(){
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage/>} />
        <Route path="/register" element={<RegisterPage/>} />

        <Route path="/" element={<ProtectedRoute><DashboardPage/></ProtectedRoute>} />
        <Route path="/dashboard" element={<ProtectedRoute><DashboardPage/></ProtectedRoute>} />
        <Route path="/conversations" element={<ProtectedRoute><ConversationsPage/></ProtectedRoute>} />
        <Route path="/conversations/:id" element={<ProtectedRoute><ChatPage/></ProtectedRoute>} />
        <Route path="/documents" element={<ProtectedRoute><DocumentsPage/></ProtectedRoute>} />

        <Route path="*" element={<Navigate to="/" replace/>} />
      </Routes>
    </AuthProvider>
  )
}
