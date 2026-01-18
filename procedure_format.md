var connection = _context.Database.GetDbConnection();
await connection.OpenAsync();

using (var command = connection.CreateCommand())
{
    command.CommandText = "GetOrderAndDetails";
    command.CommandType = CommandType.StoredProcedure;

    using (var reader = await command.ExecuteReaderAsync())
    {
        // First Result Set
        var orders = _context.Set<Order>().FromReader(reader).ToList();

        // Move to Second Result Set
        await reader.NextResultAsync();
        var details = _context.Set<OrderDetail>().FromReader(reader).ToList();
    }
}