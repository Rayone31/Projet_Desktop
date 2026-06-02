using DMsound.Session.Domain;

namespace DMsound.Session.Tests;

public sealed class SessionTests
{
    [Fact]
    public void CreateHost_initializes_a_session_with_a_single_host()
    {
        var session = global::DMsound.Session.Domain.Session.CreateHost(
            SessionId.New(),
            new SessionCode("ABCDEFGH"),
            UserId.New(),
            "Host");

        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.Single(session.Members);
        Assert.True(session.Host.IsHost);
    }

    [Fact]
    public void Join_adds_a_member_once()
    {
        var session = global::DMsound.Session.Domain.Session.CreateHost(
            SessionId.New(),
            new SessionCode("ABCDEFGH"),
            UserId.New(),
            "Host");
        var memberId = UserId.New();

        session.Join(memberId, "Guest");
        session.Join(memberId, "Guest");

        Assert.Equal(2, session.Members.Count);
    }

    [Fact]
    public void Join_throws_when_session_is_closed()
    {
        var session = global::DMsound.Session.Domain.Session.CreateHost(
            SessionId.New(),
            new SessionCode("ABCDEFGH"),
            UserId.New(),
            "Host");
        session.Close();

        Assert.Throws<InvalidOperationException>(() => session.Join(UserId.New(), "Guest"));
    }
}