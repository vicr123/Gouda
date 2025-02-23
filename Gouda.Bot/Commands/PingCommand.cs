using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Gouda.Bot.Commands;

public class PingCommand(IFeedbackService feedbackService) : CommandGroup
{
    [Command("ping")]
    [Description("Request a response")]
    public async Task<Result> Drop()
    {
        await feedbackService.SendContextualAsync("Pong!");

        return Result.Success;
    }
}
