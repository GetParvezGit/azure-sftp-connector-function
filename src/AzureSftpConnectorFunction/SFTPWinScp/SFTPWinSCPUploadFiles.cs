using AzureSftpHelper.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static AzureSftpHelper.Utils.Constants;

namespace AzureSftpConnectorFunction.SFTPWinScp
{
    public class SFTPWinSCPUploadFiles
    {
        private readonly ISftpWinSCPService _sftpWinSCPService;
        private readonly ILogger<SFTPWinSCPUploadFiles> _logger;

        public SFTPWinSCPUploadFiles(ISftpWinSCPService sftpWinSCPService, ILogger<SFTPWinSCPUploadFiles> logger)
        {
            _sftpWinSCPService = sftpWinSCPService;
            _logger = logger;
        }

        [Function(FunctionName.SFTPWinSCPUploadFiles)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
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

                await _sftpWinSCPService.UploadFile(memoryStream.ToArray(), fileName, systemName, remoteDirectory);

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