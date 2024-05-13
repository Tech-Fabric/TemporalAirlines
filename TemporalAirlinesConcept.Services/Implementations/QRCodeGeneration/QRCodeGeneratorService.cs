using QRCoder;
using SixLabors.ImageSharp;
using TemporalAirlinesConcept.Services.Models.QRCodeGeneration;

namespace TemporalAirlinesConcept.Services.Implementations.QRCodeGeneration
{
    public static class QRCodeGeneratorService
    {
        public static string Generate(QRDataModel dataModel)
        {
            using QRCodeGenerator qrGenerator = new();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(dataModel.Data, QRCodeGenerator.ECCLevel.Q);
            using QRCode qrCode = new(qrCodeData);
            using MemoryStream stream = new();

            using Image qrCodeImage = qrCode.GetGraphic(20);

            qrCodeImage.SaveAsPng(stream);

            var imageBytes = stream.ToArray();
            var base64Image = Convert.ToBase64String(imageBytes);

            return base64Image;
        }
    }
}
