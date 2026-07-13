using AzureSftpHelper.Contracts;
using AzureSftpHelper.Extensions;
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
    public class SftpSshNetService : ISftpSshNetService
    {
        private readonly IConfiguration _config;

        public SftpSshNetService(IConfiguration config)
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
                throw new InvalidOperationException($"Exception occurred listing files in SFTP directory '{remoteDirectory}'. {ex.GetMessageForLogging()}");
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
                throw new InvalidOperationException($"Exception occurred uploading file '{fileName}' to SFTP directory '{remoteDirectory}'. {ex.GetMessageForLogging()}");
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
                throw new InvalidOperationException($"Exception occurred reading file '{fileName}' from SFTP directory '{remoteDirectory}'. {ex.GetMessageForLogging()}");
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
                throw new InvalidOperationException($"Exception occurred deleting file '{fileName}' from SFTP directory '{remoteDirectory}'. {ex.GetMessageForLogging()}");
            }
        }
        #endregion

        #region Private Helper Methods
        private SftpClient CreateSftpClient(BasicConnectionConfig config)
        {
            return new SftpClient(config.Host, config.Port, config.Username, config.Password);
        }

        private BasicConnectionConfig GetConnectionConfig(string systemName)
        {
            string host = _config[$"SftpHost_SshNet_{systemName}"]!;
            string portStr = _config[$"SftpPort_SshNet_{systemName}"]!;
            string username = _config[$"SftpUsername_SshNet_{systemName}"]!;
            string password = _config[$"SftpPassword_SshNet_{systemName}"]!;

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portStr) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("One or more required SFTP configuration values are missing.");
            }

            if (!int.TryParse(portStr, out int port))
            {
                throw new InvalidOperationException($"Invalid port value: {portStr}");
            }

            return new BasicConnectionConfig
            {
                Host = host,
                Port = port,
                Username = username,
                Password = password
            };
        }
        #endregion
    }
}