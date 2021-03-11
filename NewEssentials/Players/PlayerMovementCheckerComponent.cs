using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using UnityEngine;

namespace NewEssentials.Players
{
    //TODO: If OpenMod.Games.Abstractions isn't releasing for some time add events
    public class PlayerMovementCheckerComponent : MonoBehaviour
    {
        private Vector3 m_LastLocation;
        private Player m_Player;
        private UnturnedUser m_User;

        private void Awake()
        {
            m_Player = GetComponentInParent<Player>();
            m_LastLocation = m_Player.transform.position;
        }

        public void Resolve(UnturnedUser user)
        {
            m_User = user;
        }

        private void FixedUpdate()
        {
            if (m_User == null)
            {
                return;
            }

            Vector3 newPosition = m_Player.transform.position;

            if (m_LastLocation != newPosition)
                m_User.Session.SessionData["lastMovement"] = DateTime.Now.TimeOfDay;

            m_LastLocation = newPosition;
        }
    }
}