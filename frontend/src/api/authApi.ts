import api from './axios';

export interface LoginRequest { email: string; password: string }
export interface RegisterRequest { email: string; password: string }

export async function login(req: LoginRequest){
  const res = await api.post('/auth/login', req);
  return res.data as { token: string };
}

export async function register(req: RegisterRequest){
  const res = await api.post('/auth/register', req);
  return res.data as { token: string };
}
