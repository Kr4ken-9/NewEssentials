using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API;
using NewEssentials.Models;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;

namespace NewEssentials.Managers
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class TPAManager : ITPAManager
    {
        private readonly Dictionary<ulong, HashSet<TPARequest>> m_OpenRequests = new Dictionary<ulong, HashSet<TPARequest>>();

        public TPAManager()
        {
            
        }

        public bool IsRequestOpen(ulong recipient, ulong requester)
        {
            if (!m_OpenRequests.ContainsKey(recipient))
                return false;

            return m_OpenRequests[recipient].Any(x => x.Requester == requester);
        }

        public void OpenNewRequest(ulong recipient, TPARequest request)
        {
            if (!m_OpenRequests.ContainsKey(recipient))
                m_OpenRequests.Add(recipient, new HashSet<TPARequest>());

            m_OpenRequests[recipient].Add(request);
            
            var thread = new Thread(() => RequestExpirationThread(recipient, request));
            thread.Start();
        }

        private void RequestExpirationThread(ulong recipient, TPARequest request)
        {
            Thread.Sleep(request.RequestLifetime);
            m_OpenRequests[recipient].Remove(request);
        }
    }
}