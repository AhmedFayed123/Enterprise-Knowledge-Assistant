import api from './axios';

export interface ConversationSummary { id:string; title:string; createdAt:string }
export interface ConversationDetails { id:string; title:string; createdAt:string; messages: { id:string; role:string; content:string; createdAt:string }[] }

export async function getConversations(){
  const r = await api.get('/conversations');
  return r.data as ConversationSummary[];
}
export async function getConversation(id:string){
  const r = await api.get(`/conversations/${id}`);
  return r.data as ConversationDetails;
}
export async function createConversation(title:string){
  const r = await api.post('/conversations', { title });
  return r.data as ConversationSummary;
}
export async function deleteConversation(id:string){
  const r = await api.delete(`/conversations/${id}`);
  return r;
}
export async function postMessage(conversationId:string, content:string){
  const r = await api.post(`/conversations/${conversationId}/messages`, { content });
  return r.data;
}
export function streamMessages(conversationId:string, content:string){
  // SSE endpoint
  const url = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5274/api') + `/conversations/${conversationId}/messages/stream`;
  const evtSource = new EventSource(url, { withCredentials: false } as any);
  return evtSource;
}
