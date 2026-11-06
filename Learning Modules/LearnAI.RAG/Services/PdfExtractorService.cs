using System;
using System.IO;
using System.Text;
using UglyToad.PdfPig;

namespace LearnAI.RAG.Services
{
    public class PdfExtractorService
    {
        public string ExtractTextFromStream(Stream pdfStream)
        {
            if (pdfStream == null) throw new ArgumentNullException(nameof(pdfStream));

            var contentBuilder = new StringBuilder();

            using (var document = PdfDocument.Open(pdfStream))
            {
                foreach (var page in document.GetPages())
                {
                    var pageText = page.Text;
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        contentBuilder.AppendLine(pageText);
                    }
                }
            }

            return contentBuilder.ToString();
        }
    }
}
