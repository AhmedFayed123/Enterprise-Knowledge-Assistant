using EnterpriseKnowledgeAssistant.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseKnowledgeAssistant.Application.Interfaces
{
    public interface IPdfTextExtractor
    {
        List<ExtractedPage> ExtractPages(string filePath);

    }
}
