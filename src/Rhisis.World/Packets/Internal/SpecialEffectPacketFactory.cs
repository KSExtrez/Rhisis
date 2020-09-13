﻿using Microsoft.Extensions.DependencyInjection;
using Rhisis.Core.DependencyInjection;
using Rhisis.Game.Common;
using Rhisis.Network;
using Rhisis.World.Game.Entities;

namespace Rhisis.World.Packets.Internal
{
    [Injectable(ServiceLifetime.Singleton)]
    internal class SpecialEffectPacketFactory : PacketFactoryBase, ISpecialEffectPacketFactory
    {
        /// <inheritdoc />
        public void SendSpecialEffect(IWorldEntity entity, int specialEffectId, bool sfxNoFollow)
        {
            if (entity is IPlayerEntity playerEntity)
            {
                if (playerEntity.PlayerData.Mode.HasFlag(ModeType.TRANSPARENT_MODE)) 
                {
                    return;
                }
            }

            using var packet = new FFPacket();
            
            packet.StartNewMergedPacket(entity.Id, SnapshotType.CREATESFXOBJ);
            packet.Write(specialEffectId);
            if (sfxNoFollow)
            {
                packet.Write(entity.Object.Position.X); // X
                packet.Write(entity.Object.Position.Y); // Y
                packet.Write(entity.Object.Position.Z); // Z
            }
            else
            {
                packet.Write(0f); // X
                packet.Write(0f); // Y
                packet.Write(0f); // Z
            }

            packet.Write(false); // Flag

            SendToVisible(packet, entity, sendToPlayer: true);
        }

        /// <inheritdoc />
        public void SendSpecialEffect(IWorldEntity entity, DefineSpecialEffects specialEffect, bool sfxNoFollow)
            => SendSpecialEffect(entity, (int)specialEffect, sfxNoFollow);
    }
}
