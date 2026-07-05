using Public.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Public.Application.Interface
{
    public interface IPortalServices
    {
        Task<GatewayHttpResult> PortalHealthCheck();
        Task<GatewayHttpResult> SisosLoginAsync(SisosLoginRequest request, CancellationToken cancellationToken);
    }
}
