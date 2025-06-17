using System.Threading.Tasks;
using Discord.Interactions;
using Robust.Shared.Console;

namespace Content.Server.Discord.DiscordLink.SlashCommands;

public sealed class ConsoleModule : InteractionModuleBase<SocketInteractionContext>
{
    [Dependency] private readonly IConsoleHost _console = default!;

    public ConsoleModule()
    {
        IoCManager.InjectDependencies(this);
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
            _console.ExecuteCommand(command);
        }
        catch
        {
            await FollowupAsync($"Something went wrong", ephemeral: true);
        }
    }
}
