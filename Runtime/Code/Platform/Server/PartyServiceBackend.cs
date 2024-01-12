using System.Threading.Tasks;
using Code.Http;
using Code.Http.Internal;
using Code.Platform.Shared;

namespace Code.Platform.Server
{
    [LuauAPI]
    public class PartyServiceBackend
    {
        public static async Task<HttpResponse> GetPartyForUserId(string userId)
        {
            return await InternalHttpManager.GetAsync(AirshipUrl.GameCoordinator + $"/parties/uid/{userId}");
        }

        public static async Task<HttpResponse> GetPartyById(string partyId)
        {
            return await InternalHttpManager.GetAsync(AirshipUrl.GameCoordinator + $"/parties/party-id/{partyId}");
        }
    }
}