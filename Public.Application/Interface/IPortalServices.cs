using Public.Application.Base;
using Public.Application.DTO;
using System.Text.Json;

namespace Public.Application.Interface
{
    public interface IPortalServices
    {
        Task<BaseResponse<string>> GetPortalResponseAsync(CancellationToken cancellationToken);
        Task<BaseResponse<JsonElement>> SisosLoginAsync(SisosLoginRequest request, CancellationToken cancellationToken);
        //Task<BaseResponse<JsonElement>> SisosCmdAsync(SisosCmdRequest request, CancellationToken cancellationToken);
        Task<BaseResponse<JsonElement>> SisosCustomChainAsync(SisosCustomChainRequest request, CancellationToken cancellationToken);

        Task<BaseResponse<object?>> SmsSendOTPAsync(
            SmsOTPRequest request,
            CancellationToken cancellationToken);

        Task<BaseResponse<object?>> SmsSendMessageAsync(
            SmsMessageRequest request,
            CancellationToken cancellationToken);
    }
}
