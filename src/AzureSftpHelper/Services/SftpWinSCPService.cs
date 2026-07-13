using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureSftpHelper.Extensions;
using AzureSftpHelper.Contracts;
using WinSCP;
using Session = WinSCP.Session;
using static AzureSftpHelper.Extensions.ExtensionMethods;

namespace AzureSftpHelper.Services
{
    public class SftpWinSCPService : ISftpWinSCPService
    {
        private readonly IConfiguration _config;

        public SftpWinSCPService(IConfiguration config)
        {
            _config = config;
        }

        #region List All Files
        public async Task<List<string>> ListAllFiles(string fileFilter, string systemName, string remoteDirectory)
        {
            try
            {
                var fileNames = new List<string>();
                var sessionOptions = ConfigureWinSCPSession(fileFilter, systemName, remoteDirectory);

                await Task.Run(() =>
                {
                    using (var session = new Session())
                    {
                        session.Open(sessionOptions);
                        var directoryInfo = session.ListDirectory(remoteDirectory);

                        foreach (var fileInfo in directoryInfo.Files)
                        {
                            if (!fileInfo.IsDirectory && fileInfo.Name != "." && (fileInfo.Name.StartsWith(fileFilter) || fileFilter == "*"))
                            {
                                fileNames.Add(fileInfo.Name);
                            }
                        }
                    }
                });

                return fileNames;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Exception ocurred listing files in SFTP directory '{remoteDirectory}'. {ex.GetMessageForLogging()}");
            }
        }
        #endregion

        #region Read Files
        public async Task<byte[]> ReadFiles(string fileName, string systemName, string remoteDirectory)
        {
            try
            {
                byte[] fileContent = null!;
                var sessionOptions = ConfigureWinSCPSession(fileName, systemName, remoteDirectory);

                await Task.Run(() =>
                {
                    using (var session = new Session())
                    {
                        session.Open(sessionOptions);
                        string remoteFilePath = remoteDirectory + "/" + fileName;
                        string localTempPath = Path.GetTempFileName(); // Use guaranteed temp file

                        session.GetFiles(remoteFilePath, localTempPath).Check(); // Download the file
                        fileContent = File.ReadAllBytes(localTempPath); // Read the content
                        File.Delete(localTempPath); // Clean up temp file
                    }
                });

                return fileContent!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Exception ocurred while reading file '{fileName}' from SFTP directory '{remoteDirectory}'.{ex.GetMessageForLogging()}");
            }
        }
        #endregion

        #region Delete Files
        public async Task DeleteFiles(string fileName, string systemName, string remoteDirectory)
        {
            try
            {
                var sessionOptions = ConfigureWinSCPSession(fileName, systemName, remoteDirectory);

                await Task.Run(() =>
                {
                    using (var session = new Session())
                    {
                        session.Open(sessionOptions);
                        string remoteFilePath = remoteDirectory + "/" + fileName;
                        session.RemoveFiles(remoteFilePath).Check();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Exception ocurred while deleting file '{fileName}' from SFTP directory '{remoteDirectory}'.{ex.GetMessageForLogging()}");
            }
        }

        #region Upload File
        public async Task UploadFile(byte[] fileContent, string fileName, string systemName, string remoteDirectory)
        {
            try
            {
                var sessionOptions = ConfigureWinSCPSession(fileName, systemName, remoteDirectory);

                await Task.Run(() =>
                {
                    using (var session = new Session())
                    {
                        session.Open(sessionOptions);

                        // Create a temporary file to store the content locally
                        string localTempPath = Path.GetTempFileName();
                        File.WriteAllBytes(localTempPath, fileContent);

                        try
                        {
                            // Upload the file to the remote directory
                            string remoteFilePath = $"{remoteDirectory}/{fileName}";
                            session.PutFiles(localTempPath, remoteFilePath).Check();
                        }
                        finally
                        {
                            // Clean up the temporary file
                            File.Delete(localTempPath);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Exception occurred while uploading file '{fileName}' to SFTP directory '{remoteDirectory}'. {ex.GetMessageForLogging()}");
            }
        }
        #endregion

        private SessionOptions ConfigureWinSCPSession(string fileFilterOrFileName, string systemName, string remoteDirectory)
        {
            string host = _config[$"SftpHost_WinSCP_{systemName}"]!;
            string portStr = _config[$"SftpPort_WinSCP_{systemName}"]!;
            string username = _config[$"SftpUsername_WinSCP_{systemName}"]!;
            string password = _config[$"SftpPassword_WinSCP_{systemName}"]!;

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portStr) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(remoteDirectory) ||
                string.IsNullOrWhiteSpace(fileFilterOrFileName))
            {
                throw new InvalidOperationException("One or more required SFTP configuration values or File filter is missing.");
            }

            if (!int.TryParse(portStr, out int port))
            {
                throw new InvalidOperationException($"Invalid port value: {portStr}");
            }

            return new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = host,
                PortNumber = port,
                UserName = username,
                Password = password,
                SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny
            };
        }
        #endregion
    }
}
