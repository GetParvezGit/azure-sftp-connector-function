using AzureSftpHelper.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static AzureSftpHelper.Utils.Constants;

namespace AzureSftpConnectorFunction.SFTPWinScp
{
    public class SFTPWinSCPDeleteFiles
    {
        private readonly ISftpWinSCPService _sftpWinSCPService;
        private readonly ILogger<SFTPWinSCPDeleteFiles> _logger;

        public SFTPWinSCPDeleteFiles(ISftpWinSCPService sftpWinSCPService, ILogger<SFTPWinSCPDeleteFiles> logger)
        {
            _sftpWinSCPService = sftpWinSCPService;
            _logger = logger;
        }

        [Function(FunctionName.SFTPWinSCPDeleteFiles)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] 
                                             HttpRequest req)
        {

            _logger.LogInformation("SFTPWinSCPDeleteFiles HTTP trigger function processed a request.");

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
                await _sftpWinSCPService.DeleteFiles(fileName, systemName, remoteDirectory);

                _logger.LogInformation("File '{FileName}' deleted successfully from directory '{RemoteDirectory}'.", fileName, remoteDirectory);
                
                return new OkObjectResult($"File '{fileName}' deleted successfully.");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
