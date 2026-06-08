using ObjectChess.Business.Interfaces;

namespace ObjectChess.Business.Security;

public class PasswordPolicy : IPasswordPolicy
{
    public const int MinLength = 8;

    public bool IsValid(string password, out string? error)
    {
        // First check the password is long enough
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinLength)
        {
            error = $"Password must be at least {MinLength} characters long.";
            return false;
        }

        // Must have at least one uppercase letter
        if (!password.Any(char.IsUpper))
        {
            error = "Password must contain at least one uppercase letter.";
            return false;
        }

        // Must have at least one lowercase letter
        if (!password.Any(char.IsLower))
        {
            error = "Password must contain at least one lowercase letter.";
            return false;
        }

        // Must have at least one number
        if (!password.Any(char.IsDigit))
        {
            error = "Password must contain at least one digit.";
            return false;
        }

        // A special character just means anything that is not a letter or a number (like ! ? @ #)
        if (!password.Any(c => !char.IsLetterOrDigit(c)))
        {
            error = "Password must contain at least one special character.";
            return false;
        }

        // Passed every rule so the password is good and there is no error
        error = null;
        return true;
    }
}
