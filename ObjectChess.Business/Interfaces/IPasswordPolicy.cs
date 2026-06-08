namespace ObjectChess.Business.Interfaces;

public interface IPasswordPolicy
{
    bool IsValid(string password, out string? error);
}
