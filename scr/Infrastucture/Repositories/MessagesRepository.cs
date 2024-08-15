﻿using Npgsql;
using Dapper;
using SimpleMessenger.DataAccess.Interfaces;
using SimpleMessenger.Domain;
using System.Collections.Generic;

namespace SimpleMessenger.DataAccess.Repositories;

public class MessagesRepository(string _connectionString) : IMessagesRepository
{
    private const string TableName = "Messages";

    public async Task CreateAsync(Message message)
    {
        var sql = $"""
            INSERT INTO {TableName} (id, content, created_date, sequence_number)
            VALUES (@Id, @Context, @CreatedDate, @SequenceNumber)
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, message);
    }

    public async Task<ICollection<Message>> GetMessagesInTheLastMinutesAsync(int minutes)
    {
        var sql = $"SELECT * FROM messages WHERE created_at >= NOW() - INTERVAL '{minutes} minutes'";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        var messages = await connection.QueryAsync<Message>(sql);
        return messages.ToList();
    }
}