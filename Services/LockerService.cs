using LockerApp.Data.LockerDB;
using LockerApp.Models.LockerDB;
using Microsoft.EntityFrameworkCore;

namespace LockerApp.Services
{
    public class LockerService
    {
        private readonly LockerDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;

        public LockerService(LockerDbContext dbContext, IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
        }

        // Equivalent to VBA write_inventory_transfer function
        public async Task WriteInventoryTransferAsync(
            double quantity,
            string description,
            InventoryLocation fromLocation,
            InventoryLocation toLocation,
            string company,
            string job,
            string takenFrom,
            DateTime transferDate,
            string? poNumber,
            double costPer,
            double lockerTotal,
            double inventoryTotal,
            string inspectedBy,
            IServiceProvider? serviceProvider = null)
        {
            Console.WriteLine("DEBUG: WriteInventoryTransfer() - Creating transfer record using raw SQL");
            
            // Use the provided service provider to create a new scope if needed, otherwise use the injected context
            var contextToUse = serviceProvider != null ? 
                serviceProvider.CreateScope().ServiceProvider.GetRequiredService<LockerDbContext>() : 
                _dbContext;

            // Create a transaction
            await using var transaction = await contextToUse.Database.BeginTransactionAsync();

            try
            {
                // Build the SQL with named parameters for better clarity
                var insertSql = @"
                    INSERT INTO [dbo].[inventory_transfers] 
                    ([item], [item_name], [item_desc], [from_location], [to_location], [company], [job], [taken_from], [transfer_date], [quantity], [costper], [ponum], [inspected_by])
                    VALUES (@item, @itemName, @itemDesc, @fromLocation, @toLocation, @company, @job, @takenFrom, @transferDate, @quantity, @costPer, @poNum, @inspectedBy)";
                
                // Create parameters with proper null handling
                var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>
                {
                    new ("@item", fromLocation.ItemId?.Trim() ?? (object)DBNull.Value),
                    new ("@itemName", fromLocation.ItemName?.Trim() ?? (object)DBNull.Value),
                    new ("@itemDesc", (object?)description ?? DBNull.Value),
                    new ("@fromLocation", fromLocation.Id),
                    new ("@toLocation", toLocation.Id),
                    new ("@company", (object?)company ?? DBNull.Value),
                    new ("@job", (object?)job ?? DBNull.Value),
                    new ("@takenFrom", (object?)takenFrom ?? DBNull.Value),
                    new ("@transferDate", transferDate),
                    new ("@quantity", quantity),
                    new ("@costPer", costPer),
                    new ("@poNum", (object?)poNumber ?? DBNull.Value),
                    new ("@inspectedBy", (object?)inspectedBy ?? DBNull.Value)
                };
                
                Console.WriteLine($"DEBUG: Transfer Record - ItemId: '{fromLocation.ItemId?.Trim()}', ItemName: '{fromLocation.ItemName?.Trim()}', Quantity: {quantity}");
                Console.WriteLine($"DEBUG: Transfer Record - FromLocation: {fromLocation.Id}, ToLocation: {toLocation.Id}, Date: {transferDate:yyyy-MM-dd}");
                Console.WriteLine($"DEBUG: Transfer Record - Company: '{company}', Job: '{job}', TakenFrom: '{takenFrom}', InspectedBy: '{inspectedBy}'");
                
                Console.WriteLine("DEBUG: Executing raw SQL to create transfer record (avoiding Entity Framework view conflicts)");
                var rowsAffected = await contextToUse.Database.ExecuteSqlRawAsync(insertSql, parameters);
                
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"DEBUG: Transfer record insert successful, {rowsAffected} rows affected");
                    
                    // Commit the transaction
                    await transaction.CommitAsync();
                    Console.WriteLine("DEBUG: Transaction committed successfully");
                }
                else
                {
                    Console.WriteLine("ERROR: Transfer record raw SQL insertion did not affect any rows");
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("Failed to create transfer record");
                }
            }
            catch (Exception sqlException)
            {
                Console.WriteLine($"ERROR: Raw SQL transfer record creation failed: {sqlException.Message}");
                Console.WriteLine($"Stack trace: {sqlException.StackTrace}");
                
                // Rollback the transaction on error
                try
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("DEBUG: Transaction rolled back due to error");
                }
                catch (Exception rollbackEx)
                {
                    Console.WriteLine($"ERROR: Failed to rollback transaction: {rollbackEx.Message}");
                }
                
                throw new InvalidOperationException("Unable to create transfer record", sqlException);
            }
            finally
            {
                // Dispose the context if we created a new one
                if (contextToUse != _dbContext)
                {
                    await contextToUse.DisposeAsync();
                }
            }
        }

        // Equivalent to VBA update_inv_locations function
        public async Task UpdateInventoryLocationsAsync(
            double quantity,
            InventoryLocation fromLocation,
            InventoryLocation toLocation,
            IServiceProvider? serviceProvider = null)
        {
            Console.WriteLine("DEBUG: UpdateInventoryLocations() - Starting inventory updates");
            Console.WriteLine($"DEBUG: Transfer quantity: {quantity}");
            Console.WriteLine($"DEBUG: FROM location BEFORE - ID: {fromLocation.Id}, OnHand: {fromLocation.OnHand}");
            Console.WriteLine($"DEBUG: TO location BEFORE - ID: {toLocation.Id}, OnHand: {toLocation.OnHand}");
            
            // Use the provided service provider to create a new scope if needed, otherwise use the injected context
            var contextToUse = serviceProvider != null ? 
                serviceProvider.CreateScope().ServiceProvider.GetRequiredService<LockerDbContext>() : 
                _dbContext;
                
            // Create a transaction
            await using var transaction = await contextToUse.Database.BeginTransactionAsync();
            
            try
            {
                // Decrease quantity from source location
                var originalFromOnHand = fromLocation.OnHand ?? 0;
                fromLocation.OnHand = originalFromOnHand - quantity;

                // Increase quantity at destination location  
                var originalToOnHand = toLocation.OnHand ?? 0;
                toLocation.OnHand = originalToOnHand + quantity;

                Console.WriteLine($"DEBUG: FROM location AFTER - ID: {fromLocation.Id}, OnHand: {fromLocation.OnHand} (was {originalFromOnHand})");
                Console.WriteLine($"DEBUG: TO location AFTER - ID: {toLocation.Id}, OnHand: {toLocation.OnHand} (was {originalToOnHand})");

                // Update both records
                contextToUse.InventoryLocations.Update(fromLocation);
                contextToUse.InventoryLocations.Update(toLocation);
                Console.WriteLine("DEBUG: Both inventory location records updated in context");
                
                // Save changes to the database
                var savedCount = await contextToUse.SaveChangesAsync();
                Console.WriteLine($"DEBUG: Saved {savedCount} records to database");
                
                // Commit the transaction
                await transaction.CommitAsync();
                Console.WriteLine("DEBUG: Transaction committed successfully for inventory locations update");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to update inventory locations: {ex.Message}");
                
                // Rollback the transaction on error
                try
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("DEBUG: Transaction rolled back due to error");
                }
                catch (Exception rollbackEx)
                {
                    Console.WriteLine($"ERROR: Failed to rollback transaction: {rollbackEx.Message}");
                }
                
                throw new InvalidOperationException("Unable to update inventory locations", ex);
            }
            finally
            {
                // Dispose the context if we created a new one
                if (contextToUse != _dbContext)
                {
                    await contextToUse.DisposeAsync();
                }
            }
        }
    }
}
