using QRCoder;

namespace SharingService.Services
{
    public static class QrCodeGenerator
    {
        // Generise QR kod kao Base64 PNG string - lako za slanje u JSON odgovoru,
        // frontend ga direktno prikazuje kao <img src="data:image/png;base64,..." />
        public static string GenerateQrCodeBase64(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            return System.Convert.ToBase64String(qrCodeBytes);
        }
    }
}