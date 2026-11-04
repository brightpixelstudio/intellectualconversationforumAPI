
namespace intellectualconversationAPI.Services
{
    public interface IValidateImageService
    {
        Task<string?> ValidateImagesAsync(List<IFormFile>? images, int maxPostImages = 1, long maxPostFileMaxLength = 1024 * 1024);
    }
}
