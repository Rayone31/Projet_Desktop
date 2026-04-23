using DMsound.Session.Application;
using DMsound.Session.Domain;
using DMsound.Session.Infrastructure;

namespace DMsound.Session.Tests;

public sealed class JoinSessionByCodeUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_adds_a_member_to_an_existing_session()
    {
        var repository = new InMemorySessionRepository();
        var session = global::DMsound.Session.Domain.Session.CreateHost(
            SessionId.New(),
            new SessionCode("A1B2C3D4"),
            UserId.New(),
            "Host");
        await repository.SaveAsync(session, CancellationToken.None);

        var useCase = new JoinSessionByCodeUseCase(repository);
        var result = await useCase.ExecuteAsync(new SessionCode("A1B2C3D4"), UserId.New(), "Guest", CancellationToken.None);

        Assert.Equal(2, result.MembersCount);
    }
}