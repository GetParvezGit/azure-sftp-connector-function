using AzureSftpHelper.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static AzureSftpHelper.Utils.Constants;
using System;
using System.Threading.Tasks;

namespace AzureSftpConnectorFunction.SFTPSshNet
{
    public class SFTPReadFiles
    {
        private readonly ISftpSshNetService _sftpSshNetService;
        private readonly ILogger<SFTPReadFiles> _logger;

        public SFTPReadFiles(ISftpSshNetService sftpSshNetService, ILogger<SFTPReadFiles> logger)
        {
            _sftpSshNetService = sftpSshNetService;
            _logger = logger;
        }

        [Function(FunctionName.SFTPReadFiles)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("SFTPReadAllFiles HTTP trigger function processed a request.");

            if (!req.Headers.TryGetValue("FileName", out var fileName) ||
                !req.Headers.TryGetValue("SystemName", out var systemName) ||
                !req.Headers.TryGetValue("RemoteDirectory", out var remoteDirectory))
            {
                _logger.LogError("Missing required headers: FileName: {FileName}, SystemName: {SystemName}, RemoteDirectory: {RemoteDirectory}.",
                                 fileName, systemName, remoteDirectory);

                return new BadRequestObjectResult($"Missing required headers: FileName: {fileName}, SystemName: {systemName}, RemoteDirectory: {remoteDirectory}.");
            }

            try
            {
                var fileContent = await _sftpSshNetService.ReadFile(fileName, systemName, remoteDirectory);

                return new FileContentResult(fileContent, "application/octet-stream")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file '{FileName}' from SFTP.", fileName);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}