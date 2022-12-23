using NetCord;

namespace ProgramowanieBot;

internal class TokenService
{
    public Token Token { get; }

    public TokenService(ConfigService config)
    {
        Token = new(TokenType.Bot, config.Token);
    }
}
