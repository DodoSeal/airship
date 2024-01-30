using System.Threading.Tasks;
using Code.Http;
using Code.Http.Internal;
using Code.Platform.Shared;

namespace Code.Platform.Client
{
    public class AirshipInventoryControllerBackend
    {
        public static async Task<HttpResponse> GetEquippedOutfitByUserId(string uid)
        {
            return await InternalHttpManager.GetAsync($"{AirshipUrl.ContentService}/outfits/uid/{uid}/equipped");
        }

        public static async Task<HttpResponse> GetEquippedProfilePictureByUserId(string uid)
        {
            return await InternalHttpManager.GetAsync(
                $"{AirshipUrl.ContentService}/profile-pictures/uid/{uid}/equipped");
        }
    }
}