using System;

namespace PoliNorError
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
    public class NoDelegateException : Exception
    {
        public NoDelegateException(string msg) : base(msg){}
        public NoDelegateException(IPolicyBase policy)  : this($"Delegate for policy {policy.PolicyName} was not set."){}
    }
}
