using Microsoft.Extensions.Configuration;

using NetCord;

namespace ProgramowanieBot;

internal class TokenService
{
    public Token Token { get; }

    public TokenService(IConfiguration configuration)
    {
        Token = new(TokenType.Bot, configuration.GetRequiredSection("ProgramowanieBotToken").Value);
    }
}
