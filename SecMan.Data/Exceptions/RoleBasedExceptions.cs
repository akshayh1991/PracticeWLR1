using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Exceptions
{    
    public class CommonBadRequestForRole : Exception
    {
        public CommonBadRequestForRole(string message) : base(message)
        {

        }
    }

    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }

    public class UpdatingExistingNameException : Exception
    {
        public UpdatingExistingNameException(string message) : base(message)
        {
        }
    }

    public class BadRequestForLinkUsersNotExits : Exception
    {
        public BadRequestForLinkUsersNotExits(string message) : base(message)
        {
        }
    }
}
