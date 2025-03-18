namespace dbtest;
using Data.Core;
using Data.Models;
using Data.Common;
using Microsoft.EntityFrameworkCore;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Username=postgres;Database=myDB;Port=5432");
        var context = new ApplicationDbContext(builder.Options);
        IRepository<User> userRepository  = new Repository<User>(context);
        var users  = userRepository.GetAll();
        foreach(var user in users)
            Console.WriteLine(user.uname);

    }
}
