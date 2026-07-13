using System;

namespace AzureSftpHelper.Extensions
{

    public static class ExtensionMethods
    {
        public static string GetMessageForLogging(this Exception ex)
        {
            return $"\nError Message: {ex.Message}" +
                $"\nInnerException: {ex.InnerException}" +
                $"\nStackTrace: {ex.StackTrace}";
        }
    }
}
