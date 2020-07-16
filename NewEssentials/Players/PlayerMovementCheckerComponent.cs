using System;
using NewEssentials.API.Players;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Players
{
    //TODO: If OpenMod.Games.Abstractions isn't releasing for some time add events
    public class PlayerMovementCheckerComponent : MonoBehaviour
    {
        private Vector3 lastLocation;
        private Player player;

        private IAfkChecker _checker;

        private void Awake()
        {
            player = GetComponentInParent<Player>();
            lastLocation = player.transform.position;
        }

        //Unity you are trash
        public void Resolve(IAfkChecker checker)
        {
            _checker = checker;
        }

        private void FixedUpdate()
        {
            if (lastLocation != player.transform.position)
                _checker.UpdatePlayer(player);
            lastLocation = player.transform.position;
        }
        
        
    }
}