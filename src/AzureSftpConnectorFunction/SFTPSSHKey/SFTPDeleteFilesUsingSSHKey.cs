using AzureSftpHelper.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static AzureSftpHelper.Utils.Constants;

namespace AzureSftpConnectorFunction.SFTPSSHKey;

public class SFTPDeleteFilesUsingSSHKey
{
    private readonly ILogger<SFTPDeleteFilesUsingSSHKey> _logger;
    private readonly ISftpSshKeyService _sftpSshKeyService;

    public SFTPDeleteFilesUsingSSHKey(ILogger<SFTPDeleteFilesUsingSSHKey> logger, ISftpSshKeyService sftpSshKeyService)
    {
        _logger = logger;
        _sftpSshKeyService = sftpSshKeyService;
    }

    [Function(FunctionName.SFTPDeleteFilesUsingSSHKey)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("SFTPDeleteFilesUsingSSHKey HTTP trigger function processed a request.");


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
            await _sftpSshKeyService.DeleteFile(fileName, systemName, remoteDirectory);

            return new OkObjectResult($"File '{fileName}' deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file '{FileName}' from SFTP.", fileName);
            return new BadRequestObjectResult(ex.Message);
        }
    }
}