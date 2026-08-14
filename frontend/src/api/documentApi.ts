import api from './axios';

export interface DocumentItem { id:string; fileName:string; filePath?:string; status?:string; uploadedAt?:string }

export async function getDocuments(){
  const r = await api.get('/documents');
  return r.data as DocumentItem[];
}

export async function uploadDocument(file: File){
  const form = new FormData();
  form.append('file', file);
  const r = await api.post('/documents/upload', form, { headers: { 'Content-Type': 'multipart/form-data' } });
  return r.data;
}
