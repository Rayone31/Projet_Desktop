using DMsound.Session.Application.Ports;
using DMsound.Session.Domain;

namespace DMsound.Session.Infrastructure;

public sealed class DefaultSessionCodeGenerator : ISessionCodeGenerator
{
    public string Generate() => SessionCode.Generate().Value;
}