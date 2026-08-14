import React from 'react';

export default function MessageList({messages}:{messages:{id:string; role:string; content:string; createdAt:string}[]}){
  return (
    <div className="flex flex-col gap-3">
      {messages.map(m=> (
        <div key={m.id} className={`p-3 rounded max-w-3/4 ${m.role==='user' ? 'ml-auto bg-blue-600 text-white' : 'bg-white text-gray-900'}`}>
          <div className="whitespace-pre-wrap">{m.content}</div>
          <div className="text-xs text-gray-500 mt-1">{new Date(m.createdAt).toLocaleString()}</div>
        </div>
      ))}
    </div>
  )
}
