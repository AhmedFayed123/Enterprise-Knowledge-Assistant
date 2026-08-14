export function getToken(): string | null {
  try { return localStorage.getItem('eka_token'); } catch { return null; }
}
export function setToken(token: string){ try { localStorage.setItem('eka_token', token); } catch {} }
export function clearAuth(){ try { localStorage.removeItem('eka_token'); } catch {} }
