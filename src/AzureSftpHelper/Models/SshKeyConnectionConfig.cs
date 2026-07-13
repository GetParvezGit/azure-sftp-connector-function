using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSftpHelper.Models
{
    public class SSHKeyConnectionConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string SSHKey { get; set; } = string.Empty;
        public string PassPhrase { get; set; } = string.Empty;
    }
}
