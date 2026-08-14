import React, {useEffect, useState} from 'react';
import Layout from '../components/Layout';
import { getConversations } from '../api/conversationApi';

export default function DashboardPage(){
  const [convs, setConvs] = useState<any[]>([]);
  useEffect(()=>{ (async()=>{
    try{ const data = await getConversations(); setConvs(data);}catch{}
  })(); },[]);

  return (
    <Layout>
      <div className="max-w-5xl mx-auto">
        <h1 className="text-2xl font-bold mb-4">Dashboard</h1>
        <section className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="p-4 bg-white rounded shadow">
            <h3 className="font-semibold">Conversations</h3>
            <p className="text-sm text-gray-600">You have {convs.length} conversations.</p>
          </div>
          <div className="p-4 bg-white rounded shadow">
            <h3 className="font-semibold">Quick Actions</h3>
            <div className="mt-2">
              <a href="/conversations" className="text-blue-600">Open Conversations</a>
            </div>
          </div>
        </section>
      </div>
    </Layout>
  )
}
