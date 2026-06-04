namespace core.Services;

public interface IMpvService
{
    public Task<string> StartMpv(string url);
}
