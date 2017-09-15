using System.Threading.Tasks;
using Android.Content;

namespace Xamarians.CropImage.Droid
{
    public class CropImageServiceAndroid : ICropImageService
    {
        static TaskCompletionSource<CropResult> _tcs;
        static Context _context;

        public CropImageServiceAndroid()
        {

        }
        public static void Initialize(Context context)
        {
            _context = context;
            CropImageService.Init(new CropImageServiceAndroid());
        }

        internal static void SetResult(CropResult result)
        {
            _tcs.TrySetResult(result);
        }
        public Task<CropResult> CropImage(string imagePath, CropRatioType ratioType)
        {
            _tcs = new TaskCompletionSource<CropResult>();
            Intent intent = new Intent(_context, typeof(CropImageActivity));
            intent.PutExtra("image-path", imagePath);
            intent.PutExtra("scale", true);
            if (ratioType == CropRatioType.Square)
            {
                intent.PutExtra("aspectX", 1);
                intent.PutExtra("aspectY", 1);
            }
            _context.StartActivity(intent);
            return _tcs.Task;
        }
    }
}