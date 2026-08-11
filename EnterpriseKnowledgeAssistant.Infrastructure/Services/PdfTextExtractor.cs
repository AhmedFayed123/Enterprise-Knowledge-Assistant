using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using System.Text;
using EnterpriseKnowledgeAssistant.Application.Interfaces;

namespace EnterpriseKnowledgeAssistant.Infrastructure.Services
{
    public class PdfTextExtractor : IPdfTextExtractor
    {
        public string ExtractText(string filePath)
        {
            using var document = PdfDocument.Open(filePath);

            var text = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                text.AppendLine(page.Text);
            }

            return text.ToString();
        }
    }
}
