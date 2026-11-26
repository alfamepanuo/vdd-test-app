using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public sealed class SqliteTestDb : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteConnection Connection => _connection;

    public SqliteTestDb() => _connection.Open();

    public DbContextOptions<T> Options<T>() where T : DbContext =>
        new DbContextOptionsBuilder<T>().UseSqlite(_connection).Options;

    public void Dispose() => _connection.Dispose();
}
