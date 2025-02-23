using System.ComponentModel;
using Gouda.Bot.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Gouda.Bot.Commands;

public class PingCommand(IFeedbackService feedbackService, TranslationService translationService) : CommandGroup
{
    [Command("ping")]
    [Description("Request a response")]
    public async Task<Result> Ping()
    {
        await feedbackService.SendContextualAsync(translationService["PING_RESPONSE"]);

        return Result.Success;
    }
}
