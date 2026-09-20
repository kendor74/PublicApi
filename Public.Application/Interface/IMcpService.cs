using Public.Application.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Public.Application.Interface
{
    public interface IMcpService
    {
        Task<BaseResponse<bool>> HealthCheckAsync(
            CancellationToken cancellationToken = default);

        Task<BaseResponse<string>> ConnectAsync(CancellationToken cancellationToken = default);
    }
}
