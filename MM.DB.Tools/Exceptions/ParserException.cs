using System;

namespace MM.DB.Tools.Exceptions
{
    public class ParserException: ApplicationException
    {
        public ParserException(string message) : base(message)
        {
        }
    }
}
