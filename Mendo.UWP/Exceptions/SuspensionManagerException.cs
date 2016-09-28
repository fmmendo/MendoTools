using System;

namespace Mendo.UWP.Exceptions
{
    public class SuspensionManagerException : Exception
    {
        public SuspensionManagerException()
        {
        }

        public SuspensionManagerException(Exception e) : base("SuspensionManager failed", e)
        {

        }
    }
}
