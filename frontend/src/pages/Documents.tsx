import React, {useEffect, useState} from 'react';
import Layout from '../components/Layout';
import { getDocuments, uploadDocument } from '../api/documentApi';

export default function DocumentsPage(){
  const [docs, setDocs] = useState<any[]>([]);
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(()=>{ (async()=>{ try{ setDocs(await getDocuments()); }catch{} })(); },[]);

  async function upload(){ if(!file) return; setLoading(true); try{ await uploadDocument(file); setFile(null); setDocs(await getDocuments()); }catch(err){ alert('Upload failed'); } finally{ setLoading(false); } }

  return (
    <Layout>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold mb-4">Documents</h1>
        <div className="p-4 bg-white rounded shadow mb-4">
          <input type="file" onChange={e=>setFile(e.target.files?.[0] ?? null)} />
          <button onClick={upload} disabled={loading || !file} className="ml-2 bg-blue-600 text-white px-4 rounded">Upload</button>
        </div>
        <div className="grid gap-2">
          {docs.map(d=> (
            <div key={d.id} className="p-3 bg-white rounded shadow">
              <div className="font-semibold">{d.fileName}</div>
              <div className="text-sm text-gray-500">{d.status}</div>
            </div>
          ))}
        </div>
      </div>
    </Layout>
  )
}
