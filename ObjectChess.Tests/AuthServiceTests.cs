using Xunit;
using ObjectChess.Business.Models;
using ObjectChess.Business.Security;
using ObjectChess.Business.Services;
using ObjectChess.Tests.Fakes;

namespace ObjectChess.Tests;

public class AuthServiceTests
{
    private const string ValidPassword = "StrongPass1!";

    private static AuthService CreateService(FakeUserRepository repository)
    {
        return new AuthService(repository, new PasswordHasher(), new PasswordPolicy());
    }

    [Fact]
    public void Register_ShouldSucceed_WhenEmailIsNew()
    {
        FakeUserRepository repository = new();
        AuthService authService = CreateService(repository);

        authService.Register("Emre Yozkan", "emre@example.com", ValidPassword);

        Assert.True(repository.EmailExists("emre@example.com"));
    }

    [Fact]
    public void Register_ShouldThrow_WhenEmailAlreadyExists()
    {
        FakeUserRepository repository = new();
        AuthService authService = CreateService(repository);
        repository.CreateUser(new UserModel
        {
            FullName = "Emre Yozkan",
            Email = "existing@example.com",
            PasswordHash = "hash"
        });

        Assert.Throws<ArgumentException>(() =>
            authService.Register("Emre Yozkan", "existing@example.com", ValidPassword));
    }

    [Fact]
    public void Register_ShouldThrow_WhenInputsAreEmpty()
    {
        FakeUserRepository repository = new();
        AuthService authService = CreateService(repository);

        Assert.Throws<ArgumentException>(() => authService.Register("", "", ""));
    }

    [Fact]
    public void Register_ShouldThrow_WhenPasswordIsTooWeak()
    {
        FakeUserRepository repository = new();
        AuthService authService = CreateService(repository);

        Assert.Throws<ArgumentException>(() =>
            authService.Register("Emre Yozkan", "emre@example.com", "weak"));
    }

    [Fact]
    public void Login_ShouldReturnUser_WhenCredentialsAreCorrect()
    {
        FakeUserRepository repository = new();
        AuthService authService = CreateService(repository);
        authService.Register("Emre Yozkan", "emre@example.com", ValidPassword);

        UserModel? user = authService.Login("emre@example.com", ValidPassword);

        Assert.NotNull(user);
        Assert.Equal("Emre Yozkan", user!.FullName);
    }

    [Fact]
    public void Login_ShouldReturnNull_WhenPasswordIsWrong()
    {
        FakeUserRepository repository = new();
        AuthService authService = CreateService(repository);
        authService.Register("Emre Yozkan", "emre@example.com", ValidPassword);

        UserModel? user = authService.Login("emre@example.com", "WrongPass1!");

        Assert.Null(user);
    }

    [Fact]
    public void Login_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        FakeUserRepository repository = new();
        AuthService authService = CreateService(repository);

        UserModel? user = authService.Login("ghost@example.com", ValidPassword);

        Assert.Null(user);
    }
}
