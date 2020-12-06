﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rhisis.Core.Structures.Configuration;
using Rhisis.Core.Structures.Configuration.World;
using Rhisis.Database;
using Rhisis.Game.Abstractions.Behavior;
using Rhisis.Game.Abstractions.Caching;
using Rhisis.Game.Abstractions.Entities;
using Rhisis.Game.Abstractions.Features.Chat;
using Rhisis.Game.Abstractions.Map;
using Rhisis.Game.Abstractions.Resources;
using Rhisis.Game.Resources.Loaders;
using Rhisis.Network;
using Rhisis.Network.Core.Servers;
using Rhisis.Scripting.Quests;
using Rhisis.WorldServer.Client;
using Sylver.HandlerInvoker;
using Sylver.Network.Server;
using System;
using System.Linq;

namespace Rhisis.WorldServer
{
    public sealed partial class WorldServer : NetServer<WorldServerClient>, IWorldServer
    {
        private const int ClientBufferSize = 128;
        private const int ClientBacklog = 50;

        private readonly ILogger<WorldServer> _logger;
        private readonly IGameResources _gameResources;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapManager _mapManager;
        private readonly IBehaviorManager _behaviorManager;
        private readonly IChatCommandManager _chatCommandManager;
        private readonly IRhisisDatabase _database;
        private readonly IRhisisCacheManager _cacheManager;

        public CoreConfiguration CoreConfiguration { get; }

        public WorldConfiguration WorldConfiguration { get; }

        /// <summary>
        /// Creates a new <see cref="WorldServer"/> instance.
        /// </summary>
        public WorldServer(ILogger<WorldServer> logger, IOptions<WorldConfiguration> worldConfiguration, IOptions<CoreConfiguration> coreConfiguration,
            IGameResources gameResources, IServiceProvider serviceProvider, 
            IMapManager mapManager, IBehaviorManager behaviorManager, IChatCommandManager chatCommandManager, IRhisisDatabase database, IRhisisCacheManager cacheManager)
        {
            _logger = logger;
            _gameResources = gameResources;
            _serviceProvider = serviceProvider;
            _mapManager = mapManager;
            _behaviorManager = behaviorManager;
            _chatCommandManager = chatCommandManager;
            _database = database;
            _cacheManager = cacheManager;
            CoreConfiguration = coreConfiguration.Value;
            WorldConfiguration = worldConfiguration.Value;
            PacketProcessor = new FlyffPacketProcessor();

            // TODO: remove the 0.0.0.0 when implementing lite network
            ServerConfiguration = new NetServerConfiguration("0.0.0.0", WorldConfiguration.Port, ClientBacklog, ClientBufferSize);
        }

        /// <inheritdoc />
        protected override void OnBeforeStart()
        {
            if (!_database.IsAlive())
            {
                throw new InvalidProgramException($"Cannot start {nameof(WorldServer)}. Failed to reach database.");
            }

            _gameResources.Load(typeof(DefineLoader),
                typeof(TextLoader),
                typeof(MoverLoader),
                typeof(ItemLoader),
                typeof(DialogLoader),
                typeof(ShopLoader),
                typeof(JobLoader),
                typeof(SkillLoader),
                typeof(ExpTableLoader),
                typeof(PenalityLoader),
                typeof(NpcLoader),
                typeof(QuestLoader));

            _chatCommandManager.Load();
            _behaviorManager.Load();
            _mapManager.Load();
        }

        /// <inheritdoc />
        protected override void OnAfterStart()
        {
            _logger.LogInformation("'{0}' world server is started and listenening on {1}:{2}.",
                WorldConfiguration.Name, ServerConfiguration.Host, ServerConfiguration.Port);

            IRhisisCache cache = _cacheManager.GetCache(CacheType.ClusterWorldChannels);

            cache.Set(WorldConfiguration.Id.ToString(), new WorldChannel
            {
                ClusterId = WorldConfiguration.ClusterId,
                Host = WorldConfiguration.Host,
                Port = WorldConfiguration.Port,
                Id = WorldConfiguration.Id,
                Name = WorldConfiguration.Name
            });
        }

        /// <inheritdoc />
        protected override void OnBeforeStop()
        {
            IRhisisCache cache = _cacheManager.GetCache(CacheType.ClusterWorldChannels);

            if (cache.Contains(WorldConfiguration.Id.ToString()))
            {
                cache.Delete(WorldConfiguration.Id.ToString());
            }
        }

        /// <inheritdoc />
        protected override void OnClientConnected(WorldServerClient serverClient)
        {
            serverClient.Initialize(_serviceProvider.GetRequiredService<ILogger<WorldServerClient>>(),
                _serviceProvider.GetRequiredService<IHandlerInvoker>());

            _logger.LogInformation("New client connected from {0}.", serverClient.Socket.RemoteEndPoint);
        }

        /// <inheritdoc />
        protected override void OnClientDisconnected(WorldServerClient serverClient)
        {
            _logger.LogInformation("Client disconnected from {0}.", serverClient.Socket.RemoteEndPoint);
        }

        /// <inheritdoc />
        public IPlayer GetPlayerEntity(uint id) => Clients.FirstOrDefault(x => x.Player.Id == id)?.Player;

        /// <inheritdoc />
        public IPlayer GetPlayerEntity(string name) 
            => Clients.FirstOrDefault(x => x.Player.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Player;

        /// <inheritdoc />
        public IPlayer GetPlayerEntityByCharacterId(uint id) 
            => Clients.FirstOrDefault(x => x.Player.CharacterId == id)?.Player;

        /// <inheritdoc />
        public uint GetOnlineConnectedPlayerNumber() 
            => (uint)Clients.Count();
    }
}