using LearnAI.RAG.Services;
using System;
using System.IO;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using Xunit;

namespace LearnAI.AITests.RAGServices
{
    public class PdfExtractorService_Tests
    {
        private readonly PdfExtractorService _extractorService;

        public PdfExtractorService_Tests()
        {
            _extractorService = new PdfExtractorService();
        }

        [Fact]
        public void ExtractTextFromStream_NullStream_ThrowsArgumentNullException()
        {
            // Act   // Assert
            Assert.Throws<ArgumentNullException>(() => _extractorService.ExtractTextFromStream(null!));
        }

        [Fact]
        public void ExtractTextFromStream_ValidPdfStream_ReturnsExtractedText()
        {
            // Arrange
            var expectedText = "Hello from PDF page content!";
            using var pdfStream = new MemoryStream();

            var builder = new PdfDocumentBuilder();
            var page = builder.AddPage(PageSize.A4);

            var font = builder.AddStandard14Font(Standard14Font.Helvetica);

            // Fix: Use AddText and wrap coordinates in a PdfPoint struct
            page.AddText(expectedText, 12, new PdfPoint(10, 500), font);

            var bytes = builder.Build();
            pdfStream.Write(bytes, 0, bytes.Length);
            pdfStream.Position = 0;

            // Act
            var result = _extractorService.ExtractTextFromStream(pdfStream);

            // Assert
            Assert.Contains(expectedText, result);
        }
    }
}