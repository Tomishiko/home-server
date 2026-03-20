namespace core.Models;

public abstract record AuthResult;
public record AuthSuccess(UserDto User) : AuthResult;
public record AuthFailure(string Message) : AuthResult;
