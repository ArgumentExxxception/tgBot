using Telegram.Bot;
using Telegram.Bot.Types;

namespace tgBot;

public static class TextValidation
{
    public static async Task<bool> ValidateText(Message msg)
    {
        if (msg is not {} message)
        {
            return false;
        }

        return true;
    }
}