using AzureSftpHelper.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static AzureSftpHelper.Utils.Constants;

namespace AzureSftpConnectorFunction.SFTPWinScp
{
    public class SFTPWinSCPReadFiles
    {
        private readonly ISftpWinSCPService _sftpWinSCPService;
        private readonly ILogger<SFTPWinSCPReadFiles> _logger;

        public SFTPWinSCPReadFiles(ISftpWinSCPService sftpWinSCPService, ILogger<SFTPWinSCPReadFiles> logger)
        {
            _sftpWinSCPService = sftpWinSCPService;
            _logger = logger;
        }

        [Function(FunctionName.SFTPWinSCPReadFiles)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] 
                                             HttpRequest req)
        {
            _logger.LogInformation("SFTPWinSCPReadFiles HTTP trigger function processed a request.");

            if (!req.Headers.TryGetValue("FileName", out var fileName) ||
                !req.Headers.TryGetValue("SystemName", out var systemName) ||
                !req.Headers.TryGetValue("RemoteDirectory", out var remoteDirectory))
            {
                _logger.LogError("Missing required headers: FileName: {FileName}, SystemName: {SystemName}, RemoteDirectory: {RemoteDirectory}.",
                             fileName, systemName, remoteDirectory);

                return new BadRequestObjectResult($"Missing required headers: FileName: {fileName}, SystemName: {systemName}, RemoteDirectory: {remoteDirectory}.");
            }
            else
            {
                _logger.LogInformation("Request headers retrieved successfully. FileName: {FileName}, SystemName: {SystemName}, RemoteDirectory: {RemoteDirectory}",
                                   fileName, systemName, remoteDirectory);
            }

            try
            {
                var response = await _sftpWinSCPService.ReadFiles(fileName, systemName, remoteDirectory);

                if (response == null || response.Length == 0)
                {
                    _logger.LogWarning("File '{FileName}' not found in directory '{RemoteDirectory}'.", fileName, remoteDirectory);

                    return new NotFoundObjectResult($"File '{fileName}' not found.");
                }

                // Return file as a byte array (octet-stream)
                return new FileContentResult(response, "application/octet-stream")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
