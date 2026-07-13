using AzureSftpHelper.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static AzureSftpHelper.Utils.Constants;

namespace AzureSftpConnectorFunction.SFTPSSHKey;

public class SFTPReadFilesUsingSSHKey
{
    private readonly ILogger<SFTPReadFilesUsingSSHKey> _logger;
    private readonly ISftpSshKeyService _sftpSshKeyService;

    public SFTPReadFilesUsingSSHKey(ILogger<SFTPReadFilesUsingSSHKey> logger, ISftpSshKeyService sftpSshKeyService)
    {
        _logger = logger;
        _sftpSshKeyService = sftpSshKeyService;
    }

    [Function(FunctionName.SFTPReadFilesUsingSSHKey)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("SFTPReadFilesUsingSSHKey HTTP trigger function processed a request.");

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
            var fileContent = await _sftpSshKeyService.ReadFile(fileName, systemName, remoteDirectory);

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