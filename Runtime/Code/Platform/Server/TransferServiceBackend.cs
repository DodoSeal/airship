using System.Threading.Tasks;
using Code.Http;
using Code.Http.Internal;
using Code.Platform.Shared;

namespace Code.Platform.Server
{
    [LuauAPI]
    public class TransferServiceBackend
    {
        public static Task<HttpResponse> CreateServer(string body)
        {
            return InternalHttpManager.PostAsync(
                AirshipUrl.GameCoordinator + "/servers/create",
                body
            );
        }

        public static Task<HttpResponse> Transfer(string body)
        {
            return InternalHttpManager.PostAsync(
                AirshipUrl.GameCoordinator + "/transfers/transfer",
                body
            );
        }
    }
}