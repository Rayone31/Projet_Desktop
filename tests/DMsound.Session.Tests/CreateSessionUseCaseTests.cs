using DMsound.Session.Application;
using DMsound.Session.Application.Ports;
using DMsound.Session.Domain;
using DMsound.Session.Infrastructure;

namespace DMsound.Session.Tests;

public sealed class CreateSessionUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_creates_a_session_and_returns_code()
    {
        var repository = new InMemorySessionRepository();
        var useCase = new CreateSessionUseCase(new FixedCodeGenerator("A1B2C3D4"), repository);

        var result = await useCase.ExecuteAsync(UserId.New(), "Host", CancellationToken.None);

        Assert.Equal("A1B2C3D4", result.Code);
        Assert.NotEqual(default, result.SessionId);

        var saved = await repository.GetByCodeAsync(new SessionCode(result.Code), CancellationToken.None);
        Assert.NotNull(saved);
    }

    private sealed class FixedCodeGenerator : ISessionCodeGenerator
    {
        private readonly string code;

        public FixedCodeGenerator(string code)
        {
            this.code = code;
        }

        public string Generate() => code;
    }
}