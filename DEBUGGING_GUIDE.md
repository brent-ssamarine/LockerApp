# MoveInventory Component Debugging Guide

## Console Debugging

The MoveInventory component now has comprehensive console logging. To view debug output:

### In Browser Developer Tools:
1. Open your browser's Developer Tools (F12)
2. Go to the **Console** tab
3. Trigger the Move operation
4. Look for debug messages starting with "DEBUG:" or "ERROR:"

### Key Debug Points:

#### 1. Component Initialization
- `OnParametersSetAsync()` - Shows all parameters received
- Data loading for locations, taken from options, and dates

#### 2. Form Validation
- Field validation with specific missing field names
- Quantity validation

#### 3. Database Operations
- FROM record lookup and details
- TO record lookup/creation
- Quantity sufficiency check
- Transfer record creation
- Inventory location updates
- Final save operation

#### 4. Error Handling
- Database connection issues
- Record not found scenarios
- Insufficient quantity warnings
- Exception details with stack traces

## Debugging Commands

### Check Console Output:
```javascript
// In browser console, filter for debug messages:
console.clear(); // Clear previous messages before testing
```

### Test Scenarios:

#### 1. Test with Missing Fields
- Leave ToLocation empty → Should show validation error
- Leave TakenFrom empty → Should show validation error  
- Leave AsOfDate empty → Should show validation error

#### 2. Test with Invalid Quantity
- Set TransferQuantity to 0 or negative → Should show quantity error
- Set TransferQuantity higher than available → Should show insufficient quantity error

#### 3. Test with Valid Data
- Fill all fields correctly → Should show complete execution trace

## Breakpoint Debugging in VS Code

### Setting Breakpoints:
1. Open `MoveInventory.razor` in VS Code
2. Click in the left margin next to line numbers to set breakpoints
3. Start debugging with F5

### Key Breakpoint Locations:
- Line with `HandleMove()` method start
- Line with `WriteInventoryTransfer()` call
- Line with `UpdateInventoryLocations()` call
- Line with `dbContext.SaveChangesAsync()`

## Common Issues and Solutions

### 1. "No transfer records found"
- Check if `FromLocationId` parameter is being passed correctly
- Verify the inventory location record exists in database

### 2. Database Context Issues
- Look for "A second operation was started on this context" errors
- Verify IServiceProvider pattern is working correctly

### 3. Null Reference Exceptions
- Check parameter values in `OnParametersSetAsync` debug output
- Verify all required parameters are provided by parent component

### 4. Data Loading Issues
- Check location count in debug output
- Verify database connection and queries

## Database Verification Queries

After a successful transfer, verify in database:

```sql
-- Check the transfer record was created
SELECT TOP 5 * FROM InvTrans 
ORDER BY Id DESC;

-- Check inventory locations were updated
SELECT * FROM InventoryLocations 
WHERE Id IN ([FromLocationId], [ToLocationId]);
```

## Live Debugging Tips

1. **Use Console.WriteLine extensively** - Already implemented
2. **Check browser network tab** for API calls
3. **Verify component parameters** in debug output
4. **Test with known good data** first
5. **Check database state** before and after operations

## Debug Output Examples

### Successful Transfer:
```
=== DEBUG: HandleMove() started ===
DEBUG: Component Parameters - ItemId: 'SOFT GROMMETS', ItemName: 'SOFT GROMMETS', FromLocationId: 372352, FromLocation: 'WAREHOUSE', Quantity: 25
DEBUG: Form Values - ToLocation: '123', TakenFrom: 'STOCK', AsOfDate: '1/15/2025', TransferQuantity: 5, Note: '', InspectedBy: ''
DEBUG: All validations passed, starting processing
DEBUG: Starting database operations
DEBUG: Database context created successfully
DEBUG: Found FROM record - ID: 372352, Item: 'SOFT GROMMETS', ItemName: 'SOFT GROMMETS', LocationId: 15, OnHand: 25
DEBUG: Found existing TO record - ID: 123456, OnHand: 10, LocationId: 123
DEBUG: Creating inventory transfer record
DEBUG: BEFORE Update - FROM OnHand: 25, TO OnHand: 10
DEBUG: AFTER Update - FROM OnHand: 20, TO OnHand: 15
DEBUG: Database save completed successfully
DEBUG: Transfer completed successfully, triggering callbacks
=== DEBUG: HandleMove() completed ===
```

### Error Example:
```
ERROR: Source inventory record not found for ID: 372352
ERROR: No inventory record found at source location
```

## Next Steps for Debugging

1. Run the application
2. Open browser Developer Tools Console
3. Trigger a Move operation
4. Copy the console output
5. Analyze the debug trace to identify where the issue occurs
6. Check database state if needed
