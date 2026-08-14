import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { register } from '../api/authApi';
import { useAuth } from '../context/AuthContext';

export default function RegisterPage(){
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const { login: doLogin, authenticated } = useAuth();

  if (authenticated) { navigate('/'); }

  const submit = async (e: React.FormEvent) =>{
    e.preventDefault();
    setLoading(true); setError(null);
    try{
      const res = await register({ email, password });
      doLogin(res.token);
      navigate('/');
    }catch(err:any){
      setError(err?.response?.data?.message || 'Registration failed');
    }finally{ setLoading(false); }
  }

  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="max-w-md w-full p-6 bg-white rounded shadow">
        <h1 className="text-2xl font-semibold mb-4">Register</h1>
        {error && <div className="text-red-600 mb-2">{error}</div>}
        <form onSubmit={submit}>
          <label className="block">Email</label>
          <input className="w-full p-2 border rounded mb-3" value={email} onChange={e=>setEmail(e.target.value)} />
          <label className="block">Password</label>
          <input type="password" className="w-full p-2 border rounded mb-3" value={password} onChange={e=>setPassword(e.target.value)} />
          <button disabled={loading} className="w-full bg-green-600 text-white p-2 rounded">{loading ? 'Registering...' : 'Register'}</button>
        </form>
        <div className="mt-4 text-sm text-gray-600">Already have an account? <a href="/login" className="text-blue-600">Login</a></div>
      </div>
    </div>
  )
}
