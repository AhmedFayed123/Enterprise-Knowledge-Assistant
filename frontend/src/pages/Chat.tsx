import React, {useEffect, useState, useRef} from 'react';
import Layout from '../components/Layout';
import { useParams } from 'react-router-dom';
import { getConversation, postMessage, streamMessages } from '../api/conversationApi';
import MessageList from '../components/MessageList';

export default function ChatPage(){
  const { id } = useParams();
  const [conv, setConv] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [input, setInput] = useState('');
  const [streaming, setStreaming] = useState(false);
  const evtRef = useRef<EventSource | null>(null);

  useEffect(()=>{ if(id) load(); return ()=>{ if(evtRef.current){ evtRef.current.close(); } } },[id]);
  async function load(){ if(!id) return; setLoading(true); try{ const data = await getConversation(id); setConv(data);}catch(err){ console.error(err);} finally{ setLoading(false); } }

  async function send(){ if(!id || !input) return; try{ await postMessage(id, input); setInput(''); await load(); }catch(err:any){ alert(err.response?.data?.message || 'Send failed'); } }

  function startStream(){ if(!id || !input) return; setStreaming(true); const base = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5274/api') + `/conversations/${id}/messages/stream`;
    // start SSE with POST using fetch to get cookies not used; backend expects POST with JSON; EventSource doesn't support POST, so we will use fetch with ReadableStream if possible
    // Simpler approach: call the streaming endpoint by fetch and read the body as text stream
    const controller = new AbortController();
    (async()=>{
      try{
        const res = await fetch(base, { method: 'POST', headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + localStorage.getItem('eka_token') }, body: JSON.stringify({ content: input }), signal: controller.signal });
        if(!res.ok){ throw new Error('Stream failed'); }
        const reader = res.body?.getReader();
        const decoder = new TextDecoder();
        let done=false; let accumulated='';
        while(!done){
          const r = await reader!.read();
          done = r.done ?? true;
          if(r.value){
            const chunk = decoder.decode(r.value);
            // server sends SSE frames like "data: {...}\n\n"
            const parts = chunk.split('\n\n').filter(Boolean);
            for(const p of parts){
              if(p.startsWith('data:')){
                const json = p.replace(/^data:\s*/,'');
                try{ const evt = JSON.parse(json); if(evt.type==='token'){ accumulated += evt.content; } else if(evt.type==='sources'){ /* show sources */ } else if(evt.type==='done'){ /* done */ } }
                catch{}
              }
            }
          }
        }
        await load();
      }catch(err){ console.error(err); alert('Stream error'); }
      finally{ setStreaming(false); }
    })();
  }

  return (
    <Layout>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold mb-4">{conv?.title || 'Conversation'}</h1>
        {loading && <div>Loading...</div>}
        {!loading && conv && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="md:col-span-2">
              <div className="p-4 bg-white rounded shadow mb-4">
                <MessageList messages={conv.messages} />
              </div>
              <div className="flex gap-2">
                <textarea className="flex-1 p-2 border rounded" value={input} onChange={e=>setInput(e.target.value)} />
                <div className="flex flex-col gap-2">
                  <button onClick={send} className="bg-blue-600 text-white px-4 rounded">Send</button>
                  <button onClick={startStream} disabled={streaming} className="bg-green-600 text-white px-4 rounded">Stream</button>
                </div>
              </div>
            </div>
            <aside className="p-4 bg-white rounded shadow">
              <h3 className="font-semibold">Sources</h3>
              {/* Display last message sources if available - placeholder */}
              <div className="text-sm text-gray-600">Sources will appear after responses.</div>
            </aside>
          </div>
        )}
      </div>
    </Layout>
  )
}
