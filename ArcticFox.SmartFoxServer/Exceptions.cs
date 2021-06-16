using System;

namespace ArcticFox.SmartFoxServer
{
    public class RoomFullException : Exception
    {
    }

    public class UserExistsException : Exception
    {
        public UserExistsException(string message) : base(message)
        {
        }
    }
}