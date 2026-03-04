using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetCoreSqlDb.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DotNetCoreSqlDb.Data
{
    public class MyDatabaseContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MyDatabaseContext (DbContextOptions<MyDatabaseContext> options, IHttpContextAccessor accessor)
            : base(options)
        {
            _httpContextAccessor = accessor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            if (_httpContextAccessor?.HttpContext != null)
            {
                var conn = Database.GetDbConnection() as SqlConnection;
                if (conn != null)
                {
                    conn.AccessToken = _httpContextAccessor.HttpContext.Request.Headers["X-MS-TOKEN-AAD-ACCESS-TOKEN"];
                }
            }
        }

        public DbSet<DotNetCoreSqlDb.Models.Todo> Todo { get; set; } = default!;
    }

    public class MyDatabaseContextFactory : IDesignTimeDbContextFactory<MyDatabaseContext>
    {
        public MyDatabaseContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<MyDatabaseContext>();
            var connectionString = configuration.GetConnectionString("MyDbConnection") 
                ?? configuration["AZURE_SQL_CONNECTIONSTRING"]
                ?? "Server=(local);Database=DotNetCoreSqlDb;Trusted_Connection=true;";
            
            optionsBuilder.UseSqlServer(connectionString);
            
            return new MyDatabaseContext(optionsBuilder.Options, new HttpContextAccessor());
        }
    }
}
