using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Xamarians.CropImage.Droid")]
[assembly: InternalsVisibleTo("Xamarians.CropImage.iOS")]
namespace Xamarians.CropImage
{
    public interface ICropImageService
    {
        Task<CropResult> CropImage(string imagePath, CropRatioType ratioType);
    }
}
