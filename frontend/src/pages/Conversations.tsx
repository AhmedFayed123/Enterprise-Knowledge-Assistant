import React, {useEffect, useState} from 'react';
import Layout from '../components/Layout';
import { getConversations, createConversation, deleteConversation } from '../api/conversationApi';
import { Link, useNavigate } from 'react-router-dom';

export default function ConversationsPage(){
  const [convs, setConvs] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [newTitle, setNewTitle] = useState('');
  const navigate = useNavigate();

  useEffect(()=>{ load(); },[]);
  async function load(){ setLoading(true); try{ setConvs(await getConversations()); }catch{} finally{ setLoading(false);} }

  async function create(){ if(!newTitle) return; try{ const created = await createConversation(newTitle); setNewTitle(''); navigate(`/conversations/${created.id}`); }catch(err){ console.error(err);} }

  async function del(id:string){ if(!confirm('Delete conversation?')) return; try{ await deleteConversation(id); setConvs(c=>c.filter(x=>x.id!==id)); }catch(err){ alert('Delete failed'); }}

  return (
    <Layout>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold mb-4">Conversations</h1>
        <div className="mb-4 flex gap-2">
          <input className="flex-1 p-2 border rounded" placeholder="New conversation title" value={newTitle} onChange={e=>setNewTitle(e.target.value)} />
          <button onClick={create} className="bg-blue-600 text-white px-4 rounded">Create</button>
        </div>
        {loading && <div>Loading...</div>}
        {!loading && convs.length===0 && <div className="p-4 bg-white rounded shadow">No conversations yet.</div>}
        <div className="grid gap-2">
          {convs.map(c=> (
            <div key={c.id} className="p-3 bg-white rounded shadow flex justify-between items-center">
              <div>
                <Link to={`/conversations/${c.id}`} className="font-semibold">{c.title}</Link>
                <div className="text-sm text-gray-500">{new Date(c.createdAt).toLocaleString()}</div>
              </div>
              <div>
                <button onClick={()=>del(c.id)} className="text-red-600">Delete</button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </Layout>
  )
}
