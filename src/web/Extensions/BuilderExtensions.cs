using System.Text;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Data.Core;
using Microsoft.AspNetCore.Identity;
using web.Helpers;
using Microsoft.EntityFrameworkCore;
using core.Services;
using core.Interfaces;
using core.Models;
using web.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog.Filters;
using System.Diagnostics;
using web.Models;

namespace web.Extensions;

public static class StartupExtensions
{

    //public static void LogginSetup(string DBconnection)
    //{
    //    Log.Logger = new LoggerConfiguration()
    //        .MinimumLevel.Information()
    //        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    //        .Enrich.FromLogContext()
    //        // A. Always write everything to Console
    //        .WriteTo.Console()
    //        // B. Write to DB only if "IsAudit" property is true
    //        .WriteTo.Logger(lc => lc
    //            .Filter.ByIncludingOnly(Matching.WithProperty("IsAudit", true))
    //            .WriteTo.pos(
    //                connectionString: connectionString,
    //                sinkOptions: new MSSqlServerSinkOptions
    //                {
    //                    tableName = "AuditLogs",
    //                    autoCreateSqlTable = true
    //                }
    //            ))
    //        .CreateLogger();
    //}
}


