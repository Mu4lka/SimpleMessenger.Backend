﻿using Dapper;
using Npgsql;
using SimpleMessenger.DataAccess.Interfaces;
using SimpleMessenger.Domain;

namespace Infrastucture.Repositories;

public class MessagesRepository(string _connectionString) : IMessagesRepository
{
    private const string TableName = "Messages";

    public async Task CreateAsync(Message message)
    {
        var sql = $"""
            INSERT INTO {TableName} (id, content, created_date, sequence_number)
            VALUES (@Id, @Context, @CreatedDate, @SequenceNumber)
            """;

        // TODO: Возможно лучше использовать пул соединений, чтобы каждый раз не открывать
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, message);
        await connection.CloseAsync();
    }

    public async Task<ICollection<Message>> GetMessagesInLastMinutesAsync(int minutes)
    {
        var sql = $"SELECT * FROM messages WHERE created_date >= NOW() - INTERVAL '{minutes} minutes'";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var messages = await connection.QueryAsync<Message>(sql);
        await connection.CloseAsync();
        return messages.ToList();
    }
}