using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualBasic;
using Npgsql;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ngpsql_jsonelement_issue_repro
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var db = new AppDbContext();
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await db.Database.MigrateAsync();
            }

            var models = await db.Models.ToListAsync(); //Exception occurs here
            Console.WriteLine("Finished");
        }
    }


    public class Model
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set;  }
        public JsonElement? Json { get; set; }
    }

    public class AppDbContext: DbContext
    {
        public DbSet<Model> Models { get; set; }

        protected static NpgsqlDataSource _dataSource = null!;
        protected static NpgsqlDataSource GetDataSource()
        {
            if (_dataSource != null)
                return _dataSource;


            var builder = new NpgsqlConnectionStringBuilder("Host=localhost;Port=5432;Database=npgsql_jsonelement_issue_repro;Username=postgres;")
            {
                Password = Environment.GetEnvironmentVariable("%PG_PASSWD%")
            };

            var sourceBuilder = new NpgsqlDataSourceBuilder(builder.ToString())
                .EnableDynamicJson();

            _dataSource = sourceBuilder.Build();
            return _dataSource;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder opt)
        {
            base.OnConfiguring(opt);
            opt.UseNpgsql(GetDataSource());
        }
    }
}
