namespace integrationalTests;
using System.Diagnostics;
using Data.Core;
using Data.Models;
using Data.Shared;
using core.Services;
using core.Models;
using Microsoft.EntityFrameworkCore;
public class UnitTest1
{
    DbContextOptionsBuilder<ApplicationDbContext> builder = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Username=postgres;Database=myDB;Port=5432");
    public void Test1()
    {

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Username=postgres;Database=myDB;Port=5432");
        var context = new ApplicationDbContext(builder.Options);
        IRepository<UserEntity> userRepository = new Repository<UserEntity>(context);
        var users = userRepository.GetAll();
        Console.WriteLine("First test");
        foreach (var user in users)
            Console.WriteLine(user.Uname);

    }
    //    [Fact]
    public void innerjoinTest()
    {

        var context = new ApplicationDbContext(builder.Options);
        IRepository<LogsEntity> logs = new Repository<LogsEntity>(context);
        var result = logs.Query().Include(l => l.User).ToList();
        Console.WriteLine("second test");
        foreach (var val in result)
        {
            Console.WriteLine($" user:{val.User.Uname}  event:{val.Event}");
            Assert.False(val.User == null);

        }

    }
    [Fact]
    async void AddDeleteUserTest()
    {

        var context = new ApplicationDbContext(builder.Options);

        IRepository<UserEntity> userRepo = new Repository<UserEntity>(context);
        IRepository<LogsEntity> logsRepo = new Repository<LogsEntity>(context);

        var userService = new UserService(userRepo, logsRepo, null);
        var logService = new LogService(logsRepo);

        // Add Users
        userService.NewUserAsync(new User(0, "testuname2", "12346"));
        userService.NewUserAsync(new User(0, "testuname3", "12347"));
        userService.NewUserAsync(new User(0, "testuname4", "12348"));
        userService.NewUserAsync(new User(0, "testuname5", "12349"));
        await userService.SaveChangesAsync();

        // Assert for insertion
        var inserted = userRepo.Query().Where(u => u.Uname.Contains("test")).ToList();
        Assert.Equal(4, inserted.Count());
        //Add couple of referenced logs
        uint id = userRepo.Query().Where(u=>u.Uname=="testuname2").Select(u=>u.Id).Single();
        logsRepo.AddAsync(new LogsEntity { user_id = id, Id = 0, Time = DateTime.MinValue, Event = "Event" });
        await logsRepo.SaveContextAsync();
        //Delete Users
        foreach (var user in inserted)
        {
            await userService.RemoveUserById(user.Id);
        }
        //Assert for deletetion
        var deleted = userRepo.Query().Where(u => u.Uname.Contains("name"));
        Assert.Equal(0, deleted.Count());

    }

}
