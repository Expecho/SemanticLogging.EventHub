using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SemanticLogging.EventHub.Tests.Util
{
    public static class AssertEx
    {
        public static TException Throws<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException e)
            {
                return e;
            }

            Assert.Fail("Exception of type {0} should be thrown.", typeof(TException));

            return default(TException);
        }

        public static TException ThrowsInner<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                for (Exception x = e; x != null; x = x.InnerException)
                {
                    if (x.GetType() == typeof(TException)) { return (TException)e; }
                }
            }

            Assert.Fail("Exception of type {0} should be thrown.", typeof(TException));

            return default(TException);
        }
    }

}
