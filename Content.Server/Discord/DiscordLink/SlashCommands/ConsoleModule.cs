using System.Threading.Tasks;
using Discord.Interactions;
using Robust.Shared.Console;

namespace Content.Server.Discord.DiscordLink.SlashCommands;

public sealed class ConsoleModule : InteractionModuleBase<SocketInteractionContext>
{
    [Dependency] private readonly IConsoleHost _console = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;

    public ConsoleModule()
    {
        IoCManager.InjectDependencies(this);

        _sawmill = _logManager.GetSawmill("discord.interactions.console");
    }

    [SlashCommand("sudo", "Executes the specified command in the server console")]
    [RequireRole("Maintainer")]
    public async Task SudoCommand(
        [Summary("command", "The command that will be executed in the server console")]
        string command
    )
    {
        try
        {
            await DeferAsync();
            _sawmill.Info($"Discord user {Context.User.Username} sent the command to the server console: {command}");
            _console.ExecuteCommand(command);
        }
        catch
        {
            await FollowupAsync("Something went wrong", ephemeral: true);
        }
    }
}
