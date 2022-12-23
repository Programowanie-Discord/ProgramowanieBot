using System.Text.Json;

using NetCord.Rest;

namespace ProgramowanieBot.Helpers;

internal static class RestExceptionHelper
{
    public static async Task<int> GetDiscordStatusCodeAsync(this RestException exception)
    {
        return (await JsonDocument.ParseAsync(await exception.ResponseContent.ReadAsStreamAsync())).RootElement.GetProperty("code").GetInt32();
    }
}
