using AzureSftpHelper.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static AzureSftpHelper.Utils.Constants;

namespace AzureSftpConnectorFunction.SFTPSshNet
{
    public class SFTPUploadFiles
    {
        private readonly ISftpSshNetService _sftpSshNetService;
        private readonly ILogger<SFTPUploadFiles> _logger;

        public SFTPUploadFiles(ISftpSshNetService sftpSshNetService, ILogger<SFTPUploadFiles> logger)
        {
            _sftpSshNetService = sftpSshNetService;
            _logger = logger;
        }

        [Function(FunctionName.SFTPUploadFiles)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("SFTPUploadFiles HTTP trigger function processed a request.");

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
                using var memoryStream = new MemoryStream();
                await req.Body.CopyToAsync(memoryStream);

                await _sftpSshNetService.UploadFile(memoryStream.ToArray(), fileName, systemName, remoteDirectory);

                return new OkObjectResult($"File '{fileName}' uploaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file '{FileName}' to SFTP.", fileName);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}