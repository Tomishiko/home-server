namespace integrationalTests;
using System.Diagnostics;
using Data.Core;
using core.Models;
using Data.Shared;
using Microsoft.EntityFrameworkCore;
public class UnitTest1
{
    public void Test1()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Username=postgres;Database=myDB;Port=5432");
        var context = new ApplicationDbContext(builder.Options);
        IRepository<User> userRepository  = new Repository<User>(context);
        var users  = userRepository.GetAll();
        Console.WriteLine("First test");
        foreach(var user in users)
            Console.WriteLine(user.Uname);

    }
    [Fact]
    public void innerjoinTest(){

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Username=postgres;Database=myDB;Port=5432");
        var context = new ApplicationDbContext(builder.Options);
        IRepository<Log> logs  = new Repository<Log>(context);
        var result = logs.Query().Include(l=>l.User).ToList();
        Console.WriteLine("second test");
        foreach(var val in result){
            Console.WriteLine($" user:{val.User.Uname}  event:{val.Event}");
            Assert.False(val.User == null);

        }

    }

}
