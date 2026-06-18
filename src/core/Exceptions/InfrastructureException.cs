namespace core.DomainExceptions;

public class InfrastructureException : Exception
{

    public InfrastructureException() { }
    public InfrastructureException(string msg) : base(msg) { }
    public InfrastructureException(string msg, Exception inner) : base(msg, inner) { }
}
