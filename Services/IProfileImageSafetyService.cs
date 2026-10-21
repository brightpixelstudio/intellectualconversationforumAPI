using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace intellectualconversationAPI.Services
{
    public interface IProfileImageSafetyService
    {
        Task<int> ScanProfileImageAsync(IFormFile profileImage);
    }
}
