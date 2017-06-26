
namespace Xamarians.CropImage
{
    public static class CropImageService
    {
        static ICropImageService _instance;
        public static ICropImageService Instance
        {
            get
            {
                return _instance;
            }
        }

        internal static void Init(ICropImageService cropImage)
        {
            _instance = cropImage;
        }

    }

    public class CropResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }

        public CropResult(bool success)
        {
            IsSuccess = success;
        }
    }

    public enum CropRatioType
    {
        None, Square
    }
}
