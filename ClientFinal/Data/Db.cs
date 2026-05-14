using System.Data;
using Npgsql;

namespace HospitalDbClient.Data;

public static class Db
{
    public static string ConnectionString =
        "Host=localhost;Port=5432;Username=postgres;Password=1234;Database=hospital_db;SSL Mode=Disable";

    public static NpgsqlConnection GetConnection() => new(ConnectionString);

    public static DataTable GetTable(string sql, params NpgsqlParameter[] parameters)
    {
        using var conn = GetConnection();
        using var cmd = new NpgsqlCommand(sql, conn);

        if (parameters is { Length: > 0 })
            cmd.Parameters.AddRange(parameters);

        using var adapter = new NpgsqlDataAdapter(cmd);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static int Execute(string sql, params NpgsqlParameter[] parameters)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        if (parameters is { Length: > 0 })
            cmd.Parameters.AddRange(parameters);

        return cmd.ExecuteNonQuery();
    }

    public static object? ExecuteScalar(string sql, params NpgsqlParameter[] parameters)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        if (parameters is { Length: > 0 })
            cmd.Parameters.AddRange(parameters);

        return cmd.ExecuteScalar();
    }
}