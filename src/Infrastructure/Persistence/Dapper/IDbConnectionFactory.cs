namespace Infrastructure.Persistence.Dapper;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlConnectionFactory(IOptions<ApplicationSettings> options) : IDbConnectionFactory
{
    private readonly DatabaseSettings _databaseSettings = options.Value.Databases;
    public IDbConnection CreateConnection()
        => new SqlConnection(_databaseSettings.ConnectionString);
}