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
    public class SFTPDeleteFiles
    {
        private readonly ISftpSshNetService _sftpSshNetService;
        private readonly ILogger<SFTPDeleteFiles> _logger;

        public SFTPDeleteFiles(ISftpSshNetService sftpSshNetService, ILogger<SFTPDeleteFiles> logger)
        {
            _sftpSshNetService = sftpSshNetService;
            _logger = logger;
        }

        [Function(FunctionName.SFTPDeleteFiles)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("SFTPDeleteFiles HTTP trigger function processed a request.");

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
                await _sftpSshNetService.DeleteFile(fileName, systemName, remoteDirectory);

                return new OkObjectResult($"File '{fileName}' deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file '{FileName}' from SFTP.", fileName);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}