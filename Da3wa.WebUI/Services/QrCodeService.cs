using Da3wa.Application.Interfaces;
using QRCoder;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Da3wa.WebUI.Services
{

        public class QrCodeService : IQrCodeService
        {
            private readonly IWebHostEnvironment _webHostEnvironment;

            public QrCodeService(IWebHostEnvironment webHostEnvironment)
            {
                _webHostEnvironment = webHostEnvironment;
            }

            public async Task<string> GenerateGuestInvitationAsync(int guestId, string eventName, string? eventImagePath, int guestNumber, DateTime createdDate)
            {
                // Create folder structure: wwwroot/invitations/{eventName}/
                var sanitizedEventName = SanitizeFileName(eventName);
                var invitationsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "invitations", sanitizedEventName);

                if (!Directory.Exists(invitationsFolder))
                {
                    Directory.CreateDirectory(invitationsFolder);
                }

                // Generate file name: {guestNumber}_{date}.png
                var fileName = $"{guestNumber}_{createdDate:yyyyMMdd}.png";
                var fullPath = Path.Combine(invitationsFolder, fileName);

                // Generate QR code with guest ID
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(guestId.ToString(), QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCode(qrCodeData);

                // Generate QR code bitmap (300x300)
                using var qrBitmap = qrCode.GetGraphic(20, Color.Black, Color.White, true);

                // If event has a background image, overlay QR on it
                if (!string.IsNullOrEmpty(eventImagePath))
                {
                    var eventImageFullPath = Path.Combine(_webHostEnvironment.WebRootPath, eventImagePath.TrimStart('/'));

                    if (File.Exists(eventImageFullPath))
                    {
                        await Task.Run(() => CreateInvitationWithBackground(qrBitmap, eventImageFullPath, fullPath, guestNumber, createdDate));
                    }
                    else
                    {
                        // If image doesn't exist, create simple QR with info
                        await Task.Run(() => CreateSimpleInvitation(qrBitmap, fullPath, guestNumber, createdDate));
                    }
                }
                else
                {
                    // Create simple QR with info
                    await Task.Run(() => CreateSimpleInvitation(qrBitmap, fullPath, guestNumber, createdDate));
                }

                // Return relative path
                return $"/invitations/{sanitizedEventName}/{fileName}";
            }

            public async Task DeleteInvitationAsync(string imagePath)
            {
                if (string.IsNullOrEmpty(imagePath))
                    return;

                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                }
            }

            private void CreateInvitationWithBackground(Bitmap qrBitmap, string backgroundImagePath, string outputPath, int guestNumber, DateTime createdDate)
            {
                using var backgroundImage = Image.FromFile(backgroundImagePath);

                // Create a new bitmap with the background size
                using var finalImage = new Bitmap(backgroundImage.Width, backgroundImage.Height);
                using var graphics = Graphics.FromImage(finalImage);

                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                // Draw background image
                graphics.DrawImage(backgroundImage, 0, 0, backgroundImage.Width, backgroundImage.Height);

                // Calculate QR code position (bottom-right corner with margin)
                int qrSize = Math.Min(backgroundImage.Width / 3, 300);
                int margin = 20;
                int qrX = backgroundImage.Width - qrSize - margin;
                int qrY = backgroundImage.Height - qrSize - margin;

                // Draw white background for QR code
                using (var whiteBrush = new SolidBrush(Color.White))
                {
                    graphics.FillRectangle(whiteBrush, qrX - 10, qrY - 10, qrSize + 20, qrSize + 20);
                }

                // Draw QR code
                graphics.DrawImage(qrBitmap, qrX, qrY, qrSize, qrSize);

                // Add metadata text (guest number and date) at bottom
                using (var font = new Font("Arial", 12, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                {
                    string metadata = $"Guest #{guestNumber} | {createdDate:dd/MM/yyyy}";
                    var textSize = graphics.MeasureString(metadata, font);
                    float textX = 20;
                    float textY = backgroundImage.Height - 40;

                    // Draw shadow
                    graphics.DrawString(metadata, font, shadowBrush, textX + 2, textY + 2);
                    // Draw text
                    graphics.DrawString(metadata, font, brush, textX, textY);
                }

                // Save the final image
                finalImage.Save(outputPath, ImageFormat.Png);
            }

            private void CreateSimpleInvitation(Bitmap qrBitmap, string outputPath, int guestNumber, DateTime createdDate)
            {
                int width = 600;
                int height = 700;

                using var finalImage = new Bitmap(width, height);
                using var graphics = Graphics.FromImage(finalImage);

                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.White);

                // Draw title
                using (var titleFont = new Font("Arial", 24, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(139, 115, 85)))
                {
                    string title = "Event Invitation";
                    var titleSize = graphics.MeasureString(title, titleFont);
                    graphics.DrawString(title, titleFont, brush, (width - titleSize.Width) / 2, 30);
                }

                // Draw QR code centered
                int qrSize = 350;
                int qrX = (width - qrSize) / 2;
                int qrY = 100;

                graphics.DrawImage(qrBitmap, qrX, qrY, qrSize, qrSize);

                // Draw guest info
                using (var infoFont = new Font("Arial", 16, FontStyle.Regular))
                using (var brush = new SolidBrush(Color.Black))
                {
                    string guestInfo = $"Guest #{guestNumber}";
                    var infoSize = graphics.MeasureString(guestInfo, infoFont);
                    graphics.DrawString(guestInfo, infoFont, brush, (width - infoSize.Width) / 2, qrY + qrSize + 30);

                    string dateInfo = $"Created: {createdDate:dd/MM/yyyy}";
                    var dateSize = graphics.MeasureString(dateInfo, infoFont);
                    graphics.DrawString(dateInfo, infoFont, brush, (width - dateSize.Width) / 2, qrY + qrSize + 60);
                }

                // Draw border
                using (var pen = new Pen(Color.FromArgb(139, 115, 85), 3))
                {
                    graphics.DrawRectangle(pen, 10, 10, width - 20, height - 20);
                }

                // Save the final image
                finalImage.Save(outputPath, ImageFormat.Png);
            }

            private string SanitizeFileName(string fileName)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    return "default";

                // Remove invalid characters
                var invalidChars = Path.GetInvalidFileNameChars();
                var sanitized = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());

                // Limit length
                if (sanitized.Length > 50)
                    sanitized = sanitized.Substring(0, 50);

                return sanitized.Trim();
            }
        }
    }
