namespace core.Services;

public interface IBaseDataService
{
    Task<int> SaveChangesAsync();
}
