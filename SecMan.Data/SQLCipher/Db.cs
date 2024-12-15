using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SecMan.Data.Init;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class Db : DbContext
    {
        public DbSet<SuperUser> SuperUsers { get; set; }
        public DbSet<SysFeat> SysFeats { get; set; }
        public DbSet<InitFileType> InitFileTypes { get; set; }
        public DbSet<SysFeatLang> SysFeatLangs { get; set; }
        public DbSet<SysFeatProp> SysFeatProps { get; set; }
        public DbSet<SysFeatPropLang> SysFeatPropLangs { get; set; }
        public DbSet<DevDef> DevDefs { get; set; }
        public DbSet<DevDefLang> DevDefLangs { get; set; }
        public DbSet<DevPermDef> DevPermDefs { get; set; }
        public DbSet<DevPermDefLang> DevPermDefLangs { get; set; }
        public DbSet<DevPolDef> DevPolDefs { get; set; }
        public DbSet<DevPolDefLang> DevPolDefLangs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Dev> Devs { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<DevPolVal> DevPolVals { get; set; }
        public DbSet<DevSigVal> DevSigVals { get; set; }
        public DbSet<DevPermVal> DevPermVals { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
        public DbSet<EventLogs> EventLogs { get; set; }
        public string DbPath { get; }
        public Db()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
            var dbPathConfig = configuration.GetSection("DBPath").Value;
            DbPath = System.IO.Path.Join(dbPathConfig, "SecMan.db");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                var connection = InitializeSQLiteConnection(DbPath);
                options.UseSqlite(connection);
            }
        }

        private static SqliteConnection InitializeSQLiteConnection(string databaseFile)
        {
            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json")
              .Build();
            string dbPassword = configuration["DBPassword"];
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFile,
                Password = dbPassword
            };
            return new SqliteConnection(connectionString.ToString());
        }


        // dotnet tool install --global dotnet-ef
        // dotnet add package Microsoft.EntityFrameworkCore.Design
        // dotnet ef migrations add InitialCreate
        // dotnet ef database update


        public Db(DbContextOptions<Db> options, string databaseFile)
        : base(options)
        {
            DbPath = databaseFile;
        }

        public DbSet<RSAKeys> RSAKeys { get; set; }
        public DbSet<Applications> Applications { get; set; }

    }
}

