using EnterpriseKnowledgeAssistant.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Services
{
    public class TextChunker
    {
        public List<DocumentChunkData> Split(
            List<ExtractedPage> pages,
            int chunkSize = 1000,
            int overlap = 200)
        {
            var chunks = new List<DocumentChunkData>();

            var chunkIndex = 0;

            foreach (var page in pages)
            {
                if (string.IsNullOrWhiteSpace(page.Text))
                    continue;

                var start = 0;

                while (start < page.Text.Length)
                {
                    var length = Math.Min(
                        chunkSize,
                        page.Text.Length - start);

                    var content = page.Text.Substring(start, length);

                    chunks.Add(new DocumentChunkData
                    {
                        Content = content,
                        ChunkIndex = chunkIndex++,
                        PageNumber = page.PageNumber
                    });

                    start += chunkSize - overlap;
                }
            }

            return chunks;
        }
    }
}
