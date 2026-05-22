using System;
using Xunit;
using ObjectChess.Business.Services;
using ObjectChess.Tests.Fakes;

namespace ObjectChess.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public void Register_ShouldSucceed_WhenEmailIsNew()
        {
            FakeAuthRepository fakeRepository = new FakeAuthRepository();
            AuthService authService = new AuthService(fakeRepository);
            string fullName = "Emre Yozkan";
            string email = "emre@example.com";
            string password = "StrongPassword123";

            authService.Register(fullName, email, password);

            bool isRegistered = fakeRepository.CheckIfEmailExists(email);
            Assert.True(isRegistered);
        }

        [Fact]
        public void Register_ShouldThrowException_WhenEmailAlreadyExists()
        {
            FakeAuthRepository fakeRepository = new FakeAuthRepository();
            AuthService authService = new AuthService(fakeRepository);
            string fullName = "Emre Yozkan";
            string email = "existing@example.com";
            string password = "StrongPassword123";
            fakeRepository.RegisterUser(fullName, email, "some_hashed_password");

            Action action = () => authService.Register(fullName, email, password);

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Login_ShouldReturnFullName_WhenCredentialsAreCorrect()
        {
            FakeAuthRepository fakeRepository = new FakeAuthRepository();
            AuthService authService = new AuthService(fakeRepository);
            string fullName = "Emre Yozkan";
            string email = "emre@example.com";
            string password = "StrongPassword123";
            authService.Register(fullName, email, password);

            string? resultFullName = authService.Login(email, password);

            Assert.NotNull(resultFullName);
            Assert.Equal(fullName, resultFullName);
        }

        [Fact]
        public void Login_ShouldReturnNull_WhenPasswordIsWrong()
        {
            FakeAuthRepository fakeRepository = new FakeAuthRepository();
            AuthService authService = new AuthService(fakeRepository);
            string fullName = "Emre Yozkan";
            string email = "emre@example.com";
            string password = "StrongPassword123";
            string wrongPassword = "WrongPassword456";
            authService.Register(fullName, email, password);

            string? resultFullName = authService.Login(email, wrongPassword);

            Assert.Null(resultFullName);
        }

        [Fact]
        public void Login_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            FakeAuthRepository fakeRepository = new FakeAuthRepository();
            AuthService authService = new AuthService(fakeRepository);
            
            string? resultFullName = authService.Login("ghost@example.com", "Password123");

            Assert.Null(resultFullName);
        }

        [Fact]
        public void Register_ShouldThrowException_WhenInputsAreEmpty()
        {
            FakeAuthRepository fakeRepository = new FakeAuthRepository();
            AuthService authService = new AuthService(fakeRepository);

            Action action = () => authService.Register("", "", "");

            Assert.Throws<ArgumentException>(action);
        }
    }
}