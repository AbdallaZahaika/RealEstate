using RealEstate.Models;

namespace RealEstate.Helper
{
    public class UploadHelper
    {
        public static string? UploadFile(IFormFile? imageFile, string folder = "images")
        {
            if (imageFile == null) return null;

            if (imageFile != null && imageFile.Length > 0)
            {
                // Generate a unique file name with the original extension
                var uniqueFileName = GetUniqueFileName(imageFile.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{folder}", uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }
                return $"/{folder}/" + uniqueFileName;
            }
            else
            {
                return null;
            }
        }
        public static List<string> UploadFiles(List<IFormFile> documents, string folder = "images")
        {
            List<string> urls = new List<string>();
            if (documents.Any())
            {
                foreach (var document in documents)
                {
                    // Assuming UploadHelper.UploadImage() returns the URL of the uploaded file
                    string? imageUrl = UploadHelper.UploadFile(document, folder); // Adjust if your method requires different parameters
                    if (imageUrl != null)
                    {
                        urls.Add(imageUrl);
                    }
                }
            }

            return urls;
        }
        private static string GetUniqueFileName(string fileName)
        {
            // Generate a unique name based on timestamp and random number, preserving the file extension
            var extension = Path.GetExtension(fileName);
            return DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N") + extension;
        }
    }
}