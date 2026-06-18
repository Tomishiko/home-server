namespace core.DomainExceptions;

public class DuplicateException : Exception
{

    public DuplicateException() { }
    public DuplicateException(string msg) : base(msg) { }
    public DuplicateException(string msg, Exception inner) : base(msg, inner) { }
}
