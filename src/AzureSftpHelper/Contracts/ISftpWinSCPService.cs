using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSftpHelper.Contracts
{
    public interface ISftpWinSCPService
    {
        /// <summary>
        /// List All Files
        /// </summary>
        /// <param name="SystemName"></param>
        /// <param name="fileFilter"></param>
        /// <param name="remoteDirectory"></param>
        /// <returns></returns>
        Task<List<string>> ListAllFiles(string fileFilter, string SystemName, string remoteDirectory);

        /// <summary>
        /// Read Files
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="SystemName"></param>
        /// <param name="remoteDirectory"></param>
        /// <returns></returns>
        Task<byte[]> ReadFiles(string fileName, string SystemName, string remoteDirectory);

        /// <summary>
        /// Delete Files
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="SystemName"></param>
        /// <param name="remoteDirectory"></param>
        /// <returns></returns>
        Task DeleteFiles(string fileName, string SystemName, string remoteDirectory);

        /// <summary>
        /// Uploads the file to the specified remote directory.
        /// </summary>
        /// <param name="fileContent">The content of the file to be uploaded.</param>
        /// <param name="fileName">The name of the file to be uploaded.</param>
        /// <param name="systemName">The system name.</param>
        /// <param name="remoteDirectory">The remote directory where the file will be uploaded.</param>
        /// <returns></returns>
        Task UploadFile(byte[] fileContent, string fileName, string systemName, string remoteDirectory);
    }
}
