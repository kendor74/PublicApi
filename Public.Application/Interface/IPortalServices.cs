using Public.Application.Base;
using Public.Application.DTO;

namespace Public.Application.Interface
{
    public interface IPortalServices
    {
        Task<BaseResponse<string>> GetPortalResponseAsync(CancellationToken cancellationToken);
        Task<BaseResponse<string>> SisosLoginAsync(SisosLoginRequest request, CancellationToken cancellationToken);
        Task<BaseResponse<string>> SisosCmdAsync(SisosCmdRequest request, CancellationToken cancellationToken);
        Task<BaseResponse<string>> SisosCustomChainAsync(SisosCustomChainRequest request, CancellationToken cancellationToken);
    }
}
