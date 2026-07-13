using AzureSftpHelper.Contracts;
using AzureSftpHelper.Models;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSftpHelper.Services
{
    public class SftpSshKeyService : ISftpSshKeyService
    {
        private readonly IConfiguration _config;

        public SftpSshKeyService(IConfiguration config)
        {
            _config = config;
        }

        #region List All Files
        public async Task<List<string>> GetAllFiles(string fileFilter, string systemName, string remoteDirectory)
        {
            try
            {
                var fileNames = new List<string>();

                await Task.Run(() =>
                {
                    var connectionConfig = GetConnectionConfig(systemName);

                    using (var sftpClient = CreateSftpClient(connectionConfig))
                    {
                        sftpClient.Connect();

                        var files = sftpClient.ListDirectory(remoteDirectory);

                        fileNames.AddRange(files
                            .Where(file => !file.IsDirectory &&
                                           (fileFilter == "*" || file.Name.StartsWith(fileFilter)))
                            .Select(file => file.Name));

                        sftpClient.Disconnect();
                    }
                });

                return fileNames;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Exception occurred listing files in SFTP directory '{remoteDirectory}'. {ex.Message}");
            }
        }
        #endregion

        #region Upload File
        public async Task UploadFile(byte[] fileContent, string fileName, string systemName, string remoteDirectory)
        {
            try
            {
                await Task.Run(() =>
                {
                    var connectionConfig = GetConnectionConfig(systemName);

                    using (var sftpClient = CreateSftpClient(connectionConfig))
                    {
                        sftpClient.Connect();

                        using (var memoryStream = new MemoryStream(fileContent))
                        {
                            string remoteFilePath = $"{remoteDirectory}/{fileName}";
                            sftpClient.UploadFile(memoryStream, remoteFilePath, true);
                        }

                        sftpClient.Disconnect();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Exception occurred uploading file '{fileName}' to SFTP directory '{remoteDirectory}'. {ex.Message}");
            }
        }
        #endregion

        #region Read File
        public async Task<byte[]> ReadFile(string fileName, string systemName, string remoteDirectory)
        {
            try
            {
                byte[] fileContent = null!;

                await Task.Run(() =>
                {
                    var connectionConfig = GetConnectionConfig(systemName);

                    using (var sftpClient = CreateSftpClient(connectionConfig))
                    {
                        sftpClient.Connect();

                        string remoteFilePath = $"{remoteDirectory}/{fileName}";
                        using (var memoryStream = new MemoryStream())
                        {
                            sftpClient.DownloadFile(remoteFilePath, memoryStream);
                            fileContent = memoryStream.ToArray();
                        }

                        sftpClient.Disconnect();
                    }
                });

                return fileContent;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Exception occurred reading file '{fileName}' from SFTP directory '{remoteDirectory}'. {ex.Message}");
            }
        }
        #endregion

        #region Delete File
        public async Task DeleteFile(string fileName, string systemName, string remoteDirectory)
        {
            try
            {
                await Task.Run(() =>
                {
                    var connectionConfig = GetConnectionConfig(systemName);

                    using (var sftpClient = CreateSftpClient(connectionConfig))
                    {
                        sftpClient.Connect();

                        string remoteFilePath = $"{remoteDirectory}/{fileName}";
                        sftpClient.DeleteFile(remoteFilePath);

                        sftpClient.Disconnect();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Exception occurred deleting file '{fileName}' from SFTP directory '{remoteDirectory}'. {ex.Message}");
            }
        }
        #endregion

        #region Private Helper Methods
        private SftpClient CreateSftpClient(SSHKeyConnectionConfig config)
        {
            var privateKeyFile = new PrivateKeyFile(new MemoryStream(Convert.FromBase64String(config.SSHKey)), config.PassPhrase);
            var keyFiles = new[] { privateKeyFile };

            var connectionInfo = new ConnectionInfo(config.Host, config.Port, config.Username, new PrivateKeyAuthenticationMethod(config.Username, keyFiles));
            return new SftpClient(connectionInfo);
        }

        private SSHKeyConnectionConfig GetConnectionConfig(string systemName)
        {
            string host = _config[$"SftpHost_SshKey_{systemName}"]!;
            string portStr = _config[$"SftpPort_SshKey_{systemName}"]!;
            string username = _config[$"SftpUsername_SshKey_{systemName}"]!;
            string sshKey = _config[$"SftpSSHKey_SshKey_{systemName}"]!;
            string passPhrase = _config[$"SftpPassPhrase_SshKey_{systemName}"]!;

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portStr) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(sshKey))
            {
                throw new InvalidOperationException("One or more required SFTP configuration values are missing.");
            }

            if (!int.TryParse(portStr, out int port))
            {
                throw new InvalidOperationException($"Invalid port value: {portStr}");
            }

            return new SSHKeyConnectionConfig
            {
                Host = host,
                Port = port,
                Username = username,
                SSHKey = sshKey,
                PassPhrase = passPhrase
            };
        }
        #endregion
    }
}
