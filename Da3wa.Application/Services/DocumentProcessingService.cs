using Da3wa.Application.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Presentation;
using iTextSharp.text.pdf;
using SkiaSharp;
using System.Text;

namespace Da3wa.Application.Services
{
    public class DocumentProcessingService : IDocumentProcessingService
    {
        public async Task<string> ProcessTemplateAndConvertToPngAsync(string templateFilePath, string bridegroomName, string brideName, string eventName, string outputFolder)
        {
            if (!File.Exists(templateFilePath))
            {
                throw new FileNotFoundException("Template file not found", templateFilePath);
            }

            var extension = System.IO.Path.GetExtension(templateFilePath).ToLower();
            string tempProcessedFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");

            try
            {
                // Copy template to temp location for processing
                File.Copy(templateFilePath, tempProcessedFilePath, true);

                // Process based on file type
                switch (extension)
                {
                    case ".pptx":
                        await ProcessPowerPointTemplateAsync(tempProcessedFilePath, bridegroomName, brideName);
                        break;
                    case ".docx":
                        await ProcessWordTemplateAsync(tempProcessedFilePath, bridegroomName, brideName);
                        break;
                    case ".pdf":
                        tempProcessedFilePath = await ProcessPdfTemplateAsync(tempProcessedFilePath, bridegroomName, brideName);
                        break;
                    default:
                        throw new NotSupportedException($"File type {extension} is not supported");
                }

                // Convert to PNG
                string outputFileName = $"{SanitizeFileName(eventName)}_{Guid.NewGuid()}.png";
                string outputPath = System.IO.Path.Combine(outputFolder, outputFileName);

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                await ConvertToPngAsync(tempProcessedFilePath, outputPath, extension);

                return outputPath;
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempProcessedFilePath))
                {
                    try
                    {
                        File.Delete(tempProcessedFilePath);
                    }
                    catch { }
                }
            }
        }

        private async Task ProcessPowerPointTemplateAsync(string filePath, string bridegroomName, string brideName)
        {
            await Task.Run(() =>
            {
                using (PresentationDocument presentationDocument = PresentationDocument.Open(filePath, true))
                {
                    var presentationPart = presentationDocument.PresentationPart;
                    if (presentationPart == null) return;

                    foreach (var slidePart in presentationPart.SlideParts)
                    {
                        foreach (var text in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                        {
                            if (text.Text != null)
                            {
                                text.Text = text.Text
                                    .Replace("male", bridegroomName, StringComparison.OrdinalIgnoreCase)
                                    .Replace("female", brideName, StringComparison.OrdinalIgnoreCase);
                            }
                        }
                    }

                    presentationDocument.Save();
                }
            });
        }

        private async Task ProcessWordTemplateAsync(string filePath, string bridegroomName, string brideName)
        {
            await Task.Run(() =>
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(filePath, true))
                {
                    var body = wordDocument.MainDocumentPart?.Document.Body;
                    if (body == null) return;

                    foreach (var text in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                    {
                        if (text.Text != null)
                        {
                            text.Text = text.Text
                                .Replace("male", bridegroomName, StringComparison.OrdinalIgnoreCase)
                                .Replace("female", brideName, StringComparison.OrdinalIgnoreCase);
                        }
                    }

                    wordDocument.Save();
                }
            });
        }

        private async Task<string> ProcessPdfTemplateAsync(string filePath, string bridegroomName, string brideName)
        {
            return await Task.Run(() =>
            {
                string outputPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

                using (PdfReader reader = new PdfReader(filePath))
                using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                using (PdfStamper stamper = new PdfStamper(reader, fs))
                {
                    AcroFields form = stamper.AcroFields;

                    // Try to replace text in form fields
                    foreach (var fieldName in form.Fields.Keys)
                    {
                        string fieldValue = form.GetField(fieldName);
                        if (!string.IsNullOrEmpty(fieldValue))
                        {
                            fieldValue = fieldValue
                                .Replace("male", bridegroomName, StringComparison.OrdinalIgnoreCase)
                                .Replace("female", brideName, StringComparison.OrdinalIgnoreCase);
                            form.SetField(fieldName, fieldValue);
                        }
                    }

                    stamper.FormFlattening = true;
                }

                // Delete original and return new path
                File.Delete(filePath);
                return outputPath;
            });
        }

        private async Task ConvertToPngAsync(string filePath, string outputPath, string extension)
        {
            await Task.Run(() =>
            {
                switch (extension)
                {
                    case ".pdf":
                        ConvertPdfToPng(filePath, outputPath);
                        break;
                    case ".pptx":
                    case ".docx":
                        // For Office files, we'll create a simple placeholder image
                        // In production, you might want to use a library like Aspose or LibreOffice
                        CreatePlaceholderImage(outputPath, "Invitation Card");
                        break;
                }
            });
        }

        private void ConvertPdfToPng(string pdfPath, string outputPath)
        {
            try
            {
                // Basic PDF to PNG conversion
                // Note: This is a simplified version. For production, consider using a library like PDFiumSharp
                using (var reader = new PdfReader(pdfPath))
                {
                    // Get first page
                    var pageSize = reader.GetPageSize(1);

                    // Create placeholder image with PDF dimensions
                    CreatePlaceholderImage(outputPath, "Invitation Card from PDF", (int)pageSize.Width, (int)pageSize.Height);
                }
            }
            catch
            {
                // Fallback to default size
                CreatePlaceholderImage(outputPath, "Invitation Card");
            }
        }

        private void CreatePlaceholderImage(string outputPath, string text, int width = 1920, int height = 1080)
        {
            using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                // Draw a simple gradient background
                using (var paint = new SKPaint())
                {
                    paint.Shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(width, height),
                        new SKColor[] {
                            new SKColor(139, 115, 85),  // #8B7355
                            new SKColor(160, 82, 45)    // #A0522D
                        },
                        null,
                        SKShaderTileMode.Clamp);
                    canvas.DrawRect(0, 0, width, height, paint);
                }

                // Draw text
                using (var paint = new SKPaint())
                {
                    paint.Color = SKColors.White;
                    paint.IsAntialias = true;
                    paint.TextSize = 64;
                    paint.TextAlign = SKTextAlign.Center;
                    paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);

                    canvas.DrawText(text, width / 2, height / 2, paint);
                }

                // Save image
                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(outputPath))
                {
                    data.SaveTo(stream);
                }
            }
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(sanitized) ? "invitation" : sanitized;
        }
    }
}
