using System;

namespace NewEssentials.Models
{
    public class TPARequest
    {
        public ulong Requester;
        public DateTime RequestDate;
        public int RequestLifetime;
        
        public TPARequest(ulong requester, DateTime requestDate, int requestLifetime)
        {
            Requester = requester;
            RequestDate = requestDate;
            RequestLifetime = requestLifetime;
        }
    }
}