using System;
using NewEssentials.API.Players;
using OpenMod.Core.Helpers;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Players
{
    //TODO: If OpenMod.Games.Abstractions isn't releasing for some time add events
    public class PlayerMovementCheckerComponent : MonoBehaviour
    {
        private Vector3 m_LastLocation;
        private Player m_Player;

        private IAfkChecker m_Checker;

        private void Awake()
        {
            m_Player = GetComponentInParent<Player>();
            m_LastLocation = m_Player.transform.position;
        }

        //Unity you are trash
        public void Resolve(IAfkChecker checker)
        {
            m_Checker = checker;
        }

        private void FixedUpdate()
        {
            if (m_LastLocation != m_Player.transform.position)
               AsyncHelper.RunSync(async () => await m_Checker.UpdatePlayer(m_Player));
            
            m_LastLocation = m_Player.transform.position;
        }
        
        
    }
}