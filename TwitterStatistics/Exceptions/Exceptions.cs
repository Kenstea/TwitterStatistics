using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterStatistics.Service.Exceptions
{
    public class InvalidResponseException:Exception
    {
        public InvalidResponseException(string message) :base(message)
        {
        }
    }

    public class InvalidUrlException : ArgumentException
    {
        public InvalidUrlException(string message, string paramName) : base(message,paramName)
        {

        }
    }
}
