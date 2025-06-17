using System.Reflection;
using System.Threading.Tasks;
using Content.Shared.CCVar;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;
using LogMessage = Discord.LogMessage;

namespace Content.Server.Discord.DiscordLink;

/// <summary>
/// Handles the connection to Discord and provides methods to interact with it.
/// </summary>
public sealed class DiscordLink : IPostInjectInit
{
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;

    /// <summary>
    ///    The Discord client. This is null if the bot is not connected.
    /// </summary>
    /// <remarks>
    ///     This should not be used directly outside of DiscordLink. So please do not make it public. Use the methods in this class instead.
    /// </remarks>
    private DiscordSocketClient? _client;
    private InteractionService? _interaction;
    private IServiceProvider? _serviceProvider;

    private ISawmill _sawmill = default!;
    private ISawmill _sawmillLog = default!;

    private ulong _guildId;
    private string _botToken = string.Empty;

    public string BotPrefix = default!;
    /// <summary>
    /// If the bot is currently connected to Discord.
    /// </summary>
    public bool IsConnected => _client != null;

    #region Events

    /// <summary>
    ///     Event that is raised when a message is received from Discord. This is raised for every message, including commands.
    /// </summary>
    public event Action<SocketMessage>? OnMessageReceived;

    #endregion

    public void Initialize()
    {
        _configuration.OnValueChanged(CCVars.DiscordGuildId, OnGuildIdChanged, true);
        _configuration.OnValueChanged(CCVars.DiscordPrefix, OnPrefixChanged, true);

        if (_configuration.GetCVar(CCVars.DiscordToken) is not { } token || token == string.Empty)
        {
            _sawmill.Info("No Discord token specified, not connecting.");
            return;
        }

        // If the Guild ID is empty OR the prefix is empty, we don't want to connect to Discord.
        if (_guildId == 0 || BotPrefix == string.Empty)
        {
            // This is a warning, not info, because it's a configuration error.
            // It is valid to not have a Discord token set which is why the above check is an info.
            // But if you have a token set, you should also have a guild ID and prefix set.
            _sawmill.Warning("No Discord guild ID or prefix specified, not connecting.");
            return;
        }

        _client = new DiscordSocketClient(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.Guilds
                             | GatewayIntents.GuildMembers
                             | GatewayIntents.GuildMessages
                             | GatewayIntents.MessageContent
                             | GatewayIntents.DirectMessages,
        });

        _interaction = new InteractionService(_client);

        _serviceProvider = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_interaction)
            .BuildServiceProvider();

        _client.Log += Log;
        _client.MessageReceived += OnMessageReceivedInternal;
        _client.InteractionCreated += HandleInteraction;

        _botToken = token;
        // Since you cannot change the token while the server is running / the DiscordLink is initialized,
        // we can just set the token without updating it every time the cvar changes.

        _client.Ready += () =>
        {
            _sawmill.Info("Discord client ready.");
            RegisterSlashCommands();
            return Task.CompletedTask;
        };

        Task.Run(async () =>
        {
            try
            {
                await LoginAsync(token);
            }
            catch (Exception e)
            {
                _sawmill.Error("Failed to connect to Discord!", e);
            }
        });
    }

    private async Task RegisterSlashCommands()
    {
        if (_interaction == null)
            return;

        _sawmill.Info("Registering slash commands...");
        await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        await _interaction.RegisterCommandsToGuildAsync(_guildId);
        _sawmill.Info("Slash commands registered.");
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        if (_interaction == null)
            return;

        var context = new SocketInteractionContext(_client, interaction);
        await _interaction.ExecuteCommandAsync(context, _serviceProvider);
    }

    public async Task Shutdown()
    {
        if (_client != null)
        {
            _sawmill.Info("Disconnecting from Discord.");

            // Unsubscribe from the events.
            _client.MessageReceived -= OnMessageReceivedInternal;
            _client.InteractionCreated -= HandleInteraction;

            await _client.LogoutAsync();
            await _client.StopAsync();
            await _client.DisposeAsync();
            _client = null;
        }

        _configuration.UnsubValueChanged(CCVars.DiscordGuildId, OnGuildIdChanged);
        _configuration.UnsubValueChanged(CCVars.DiscordPrefix, OnPrefixChanged);
    }

    void IPostInjectInit.PostInject()
    {
        _sawmill = _logManager.GetSawmill("discord.link");
        _sawmillLog = _logManager.GetSawmill("discord.link.log");
    }

    private void OnGuildIdChanged(string guildId)
    {
        _guildId = ulong.TryParse(guildId, out var id) ? id : 0;
    }

    private void OnPrefixChanged(string prefix)
    {
        BotPrefix = prefix;
    }

    private async Task LoginAsync(string token)
    {
        DebugTools.Assert(_client != null);
        DebugTools.Assert(_client.LoginState == LoginState.LoggedOut);

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();


        _sawmill.Info("Connected to Discord.");
    }

    private string FormatLog(LogMessage msg)
    {
        return msg.Exception is null
            ? $"{msg.Source}: {msg.Message}"
            : $"{msg.Source}: {msg.Message}\n{msg.Exception}";
    }

    private Task Log(LogMessage msg)
    {
        var logLevel = msg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Fatal,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            _ => LogLevel.Debug
        };

        _sawmillLog.Log(logLevel, FormatLog(msg));
        return Task.CompletedTask;
    }

    private Task OnMessageReceivedInternal(SocketMessage message)
    {
        OnMessageReceived?.Invoke(message);
        return Task.CompletedTask;
    }

    #region Proxy methods

    /// <summary>
    /// Sends a message to a Discord channel with the specified ID. Without any mentions.
    /// </summary>
    public async Task SendMessageAsync(ulong channelId, string message)
    {
        if (_client == null)
        {
            return;
        }

        var channel = _client.GetChannel(channelId) as IMessageChannel;
        if (channel == null)
        {
            _sawmill.Error("Tried to send a message to Discord but the channel {Channel} was not found.", channel);
            return;
        }

        await channel.SendMessageAsync(message, allowedMentions: AllowedMentions.None);
    }

    #endregion
}
