using AzureSftpHelper.Contracts;
using AzureSftpHelper.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static AzureSftpHelper.Utils.Constants;

namespace AzureSftpConnectorFunction.SFTPSshNet;

public class SFTPGetAllFiles
{
    private readonly ISftpSshNetService _sftpSshNetService;
    private readonly ILogger<SFTPGetAllFiles> _logger;

    public SFTPGetAllFiles(ISftpSshNetService sftpSshNetService, ILogger<SFTPGetAllFiles> logger)
    {
        _sftpSshNetService = sftpSshNetService;
        _logger = logger;
    }

    [Function(FunctionName.SFTPGetAllFiles)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("SFTPWinSCPListFiles HTTP trigger function processed a request.");

        if (!req.Headers.TryGetValue("FileNamePattern", out var fileFilter) ||
            !req.Headers.TryGetValue("SystemName", out var systemName) ||
            !req.Headers.TryGetValue("RemoteDirectory", out var remoteDirectory))
        {
            _logger.LogError("Missing required headers: FileFilter: {FileFilter}, SystemName: {SystemName}, RemoteDirectory: {RemoteDirectory}.",
                         fileFilter, systemName, remoteDirectory);

            return new BadRequestObjectResult($"Missing required headers: FileFilter: {fileFilter}, SystemName: {systemName}, RemoteDirectory: {remoteDirectory}.");
        }
        else
        {
            _logger.LogInformation("Request headers retrieved successfully. FileFilter: {FileFilter}, SystemName: {SystemName}, RemoteDirectory: {RemoteDirectory}",
                               fileFilter, systemName, remoteDirectory);
        }

        try
        {
            var response = await _sftpSshNetService.GetAllFiles(fileFilter, systemName, remoteDirectory);

            var result = new { FilteredFileNames = response };

            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}