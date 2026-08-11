using EnterpriseKnowledgeAssistant.Application.Interfaces;
using EnterpriseKnowledgeAssistant.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace EnterpriseKnowledgeAssistant.Infrastructure.Services
{
    public class PdfTextExtractor : IPdfTextExtractor
    {
        public List<ExtractedPage> ExtractPages(string filePath)
        {
            using var document = PdfDocument.Open(filePath);

            var pages = new List<ExtractedPage>();

            foreach (var page in document.GetPages())
            {
                pages.Add(new ExtractedPage
                {
                    PageNumber = page.Number,
                    Text = page.Text
                });
            }

            return pages;
        }
    }
}
