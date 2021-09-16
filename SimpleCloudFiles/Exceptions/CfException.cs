using System;

namespace SimpleCloudFiles.Exceptions
{
    public class CfException : Exception
    {
        public CfException(string message) : base(message) { }
        public CfException(string message, Exception inner) : base(message, inner) { }
        public CfExceptionCodeEnum Code { get; set; }
    }
}
