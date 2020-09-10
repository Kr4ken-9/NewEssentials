using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands
{
    [Command("compass")]
    [CommandDescription("Get the direction you are facing or turn to face a specific direction")]
    [CommandSyntax("[North/South/East/West]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CCompass : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private const ushort North = (ushort) EDirections.NORTH;
        private const ushort NorthEast = (ushort) EDirections.NORTH_EAST;
        private const ushort East = (ushort) EDirections.EAST;
        private const ushort SouthEast = (ushort) EDirections.SOUTH_EAST;
        private const ushort South = (ushort) EDirections.SOUTH;
        private const ushort SouthWest = (ushort) EDirections.SOUTH_WEST;
        private const ushort West = (ushort) EDirections.WEST;
        private const ushort NorthWest = (ushort) EDirections.NORTH_WEST;

        public CCompass(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            float direction = uPlayer.Player.Player.transform.eulerAngles.y;
            string directionName;

            //TODO: Teleporting breaks direction name for some reason
            if (Context.Parameters.Length == 1)
            {
                direction = Context.Parameters[0].ToUpperInvariant() switch
                {
                    "N" => 0,
                    "NORTH" => 0,
                    "S" => 180,
                    "SOUTH" => 180,
                    "E" => 90,
                    "EAST" => 90,
                    "W" => 270,
                    "WEST" => 270,
                    _ => throw new CommandWrongUsageException(Context)
                };

                await uPlayer.Player.Player.TeleportToLocationAsync(uPlayer.Player.Player.transform.position, direction);
            }

            directionName = direction switch
            {
                _ when direction < North || direction > NorthWest => m_StringLocalizer["compass:north"],
                _ when direction > North && direction < NorthEast => m_StringLocalizer["compass:northeast"],
                _ when direction > NorthEast && direction < East => m_StringLocalizer["compass:east"],
                _ when direction > East && direction < SouthEast => m_StringLocalizer["compass:southeast"],
                _ when direction > SouthEast && direction < South => m_StringLocalizer["compass:south"],
                _ when direction > South && direction < SouthWest => m_StringLocalizer["compass:southwest"],
                _ when direction > SouthWest && direction < West => m_StringLocalizer["compass:west"],
                _ when direction > West && direction < NorthWest => m_StringLocalizer["compass:northwest"],
                _ => m_StringLocalizer["compass:unknown"]
            };

            await uPlayer.PrintMessageAsync(directionName);
        }
    }

    // These are actually halfway points between each real direction
    public enum EDirections
    {
        NORTH = 30,
        NORTH_EAST = 60,
        EAST = 120,
        SOUTH_EAST = 150,
        SOUTH = 210,
        SOUTH_WEST = 240,
        WEST = 300,
        NORTH_WEST = 330
    }
}