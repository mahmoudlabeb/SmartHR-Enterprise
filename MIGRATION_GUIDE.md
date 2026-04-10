# SmartHR - Database Migration Guide

## Required Actions After Code Updates

The code improvements introduced a database schema change that requires a migration. This guide walks you through the process.

---

## ?? What Changed?

The `BaseEntity` model now includes additional audit fields:

```csharp
// NEW Fields Added:
public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
public DateTime? DeletedAt { get; set; }
```

These fields will be added to **all tables** that inherit from `BaseEntity`:
- Employees
- Departments
- Designations
- Leaves
- Attendance
- Projects

---

## ?? Step-by-Step Migration Process

### Step 1: Create the Migration
Open PowerShell/Command Prompt in the SmartHR project directory and run:

```bash
cd C:\Users\user\Desktop\HR\SmartHR
dotnet ef migrations add AddAuditTrailToBaseEntity
```

**Expected Output:**
```
To undo this action, use 'dotnet ef migrations remove'
```

This creates a new file: `Migrations\[Timestamp]_AddAuditTrailToBaseEntity.cs`

### Step 2: Review the Migration
The generated migration file will look similar to:

```csharp
public partial class AddAuditTrailToBaseEntity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add UpdatedAt column to all tables
        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "Employees",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        
        // Add DeletedAt column (nullable)
        migrationBuilder.AddColumn<DateTime>(
            name: "DeletedAt",
            table: "Employees",
            type: "datetime2",
            nullable: true);
        
        // ... similar for other tables
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Rollback operations
        migrationBuilder.DropColumn(name: "UpdatedAt", table: "Employees");
        migrationBuilder.DropColumn(name: "DeletedAt", table: "Employees");
        // ... etc
    }
}
```

### Step 3: Apply the Migration to Database

```bash
dotnet ef database update
```

**Expected Output:**
```
Done. Successful database update.
```

### Step 4: Verify the Changes

Connect to your database and verify:

```sql
-- Check Employees table structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Employees'
ORDER BY ORDINAL_POSITION;
```

You should see three new columns:
- `UpdatedAt` (datetime2, NOT NULL)
- `DeletedAt` (datetime2, NULL)

---

## ?? Rollback Process (If Needed)

If you need to revert the migration:

```bash
# Revert to previous migration
dotnet ef database update [PreviousMigrationName]

# Or remove the migration entirely
dotnet ef migrations remove
```

---

## ?? Database Changes Summary

### Tables Affected
The following tables will receive the new columns:

| Table | New Columns | Purpose |
|-------|-------------|---------|
| Employees | UpdatedAt, DeletedAt | Audit trail & soft delete tracking |
| Departments | UpdatedAt, DeletedAt | Audit trail & soft delete tracking |
| Designations | UpdatedAt, DeletedAt | Audit trail & soft delete tracking |
| Leaves | UpdatedAt, DeletedAt | Audit trail & soft delete tracking |
| Attendance | UpdatedAt, DeletedAt | Audit trail & soft delete tracking |
| Projects | UpdatedAt, DeletedAt | Audit trail & soft delete tracking |

### Column Specifications

**UpdatedAt:**
```sql
ALTER TABLE Employees ADD UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE();
-- Type: datetime2 (UTC)
-- Nullable: No
-- Default: UTC current time
```

**DeletedAt:**
```sql
ALTER TABLE Employees ADD DeletedAt datetime2 NULL;
-- Type: datetime2 (UTC)
-- Nullable: Yes (NULL = not deleted, has value = deleted at that time)
```

---

## ?? Post-Migration: Update Code

### 1. Update Controllers to Set UpdatedAt

When modifying entities, set the UpdatedAt timestamp:

```csharp
[HttpPost]
public async Task<IActionResult> Edit(int id, EmployeeViewModel model)
{
    var employee = await _context.Employees.FindAsync(id);
    if (employee == null)
        return NotFound();

    employee.FullName = model.FullName;
    employee.Email = model.Email;
    employee.PhoneNumber = model.PhoneNumber;
    
    // ? NEW: Set UpdatedAt timestamp
    employee.UpdatedAt = DateTime.UtcNow;

    _context.Update(employee);
    await _context.SaveChangesAsync();

    return RedirectToAction("Index");
}
```

### 2. Soft Delete: Set Both Flags

When soft-deleting an entity:

```csharp
[HttpPost]
public async Task<IActionResult> Delete(int id)
{
    var employee = await _context.Employees.FindAsync(id);
    if (employee == null)
        return NotFound();

    // ? NEW: Set both flags
    employee.IsDeleted = true;
    employee.DeletedAt = DateTime.UtcNow;
    employee.UpdatedAt = DateTime.UtcNow;

    _context.Update(employee);
    await _context.SaveChangesAsync();

    return RedirectToAction("Index");
}
```

### 3. Optional: Add Interceptor for Automatic UpdatedAt

To automatically set UpdatedAt on every save, add this to `SmartHRContext.cs`:

```csharp
public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
{
    var entities = ChangeTracker.Entries<BaseEntity>();
    
    foreach (var entity in entities)
    {
        if (entity.State == EntityState.Modified)
        {
            // Automatically update the UpdatedAt timestamp
            entity.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
    
    return await base.SaveChangesAsync(cancellationToken);
}
```

---

## ?? Testing the Migration

### Test 1: Create Records
```csharp
[Test]
public async Task CreateEmployee_SetsAuditFields()
{
    var employee = new Employee
    {
        FullName = "John Doe",
        Email = "john@example.com",
        DepartmentId = 1,
        DesignationId = 1
    };
    
    _context.Employees.Add(employee);
    await _context.SaveChangesAsync();
    
    // Verify timestamps were set
    Assert.IsNotNull(employee.CreatedAt);
    Assert.IsNotNull(employee.UpdatedAt);
    Assert.IsNull(employee.DeletedAt);
}
```

### Test 2: Update Records
```csharp
[Test]
public async Task UpdateEmployee_UpdatesTimestamp()
{
    var employee = await _context.Employees.FirstAsync();
    var originalUpdatedAt = employee.UpdatedAt;
    
    await Task.Delay(100); // Ensure time difference
    
    employee.FullName = "Jane Doe";
    employee.UpdatedAt = DateTime.UtcNow;
    
    _context.Update(employee);
    await _context.SaveChangesAsync();
    
    // Verify UpdatedAt changed
    Assert.IsTrue(employee.UpdatedAt > originalUpdatedAt);
}
```

### Test 3: Soft Delete
```csharp
[Test]
public async Task SoftDeleteEmployee_SetsDeletedFlags()
{
    var employee = await _context.Employees.FirstAsync();
    
    employee.IsDeleted = true;
    employee.DeletedAt = DateTime.UtcNow;
    
    _context.Update(employee);
    await _context.SaveChangesAsync();
    
    // Verify soft delete
    Assert.IsTrue(employee.IsDeleted);
    Assert.IsNotNull(employee.DeletedAt);
}
```

---

## ?? Querying with New Fields

### Find Recently Updated Records
```csharp
var lastWeek = DateTime.UtcNow.AddDays(-7);
var recentUpdates = await _context.Employees
    .Where(e => e.UpdatedAt > lastWeek && !e.IsDeleted)
    .OrderByDescending(e => e.UpdatedAt)
    .ToListAsync();
```

### Find Soft-Deleted Records
```csharp
var deleted = await _context.Employees
    .IgnoreQueryFilters() // Important: ignore the soft-delete filter
    .Where(e => e.IsDeleted)
    .ToListAsync();
```

### Audit Report
```csharp
var auditReport = await _context.Employees
    .IgnoreQueryFilters()
    .Select(e => new
    {
        e.Id,
        e.FullName,
        CreatedDate = e.CreatedAt.ToLocalTime(),
        UpdatedDate = e.UpdatedAt.ToLocalTime(),
        DeletedDate = e.DeletedAt.HasValue ? e.DeletedAt.Value.ToLocalTime() : (DateTime?)null,
        Status = e.IsDeleted ? "Deleted" : "Active"
    })
    .OrderByDescending(x => x.UpdatedDate)
    .ToListAsync();
```

---

## ? Migration Checklist

Before and after migration:

### Before Running Migration
- [ ] Backup your database
- [ ] Stop the application
- [ ] Commit code changes to version control
- [ ] Review the generated migration file
- [ ] Test migration on a development database first

### After Running Migration
- [ ] Verify columns exist in database
- [ ] Verify no data loss occurred
- [ ] Start the application
- [ ] Test CRUD operations
- [ ] Monitor logs for errors
- [ ] Update controllers to set UpdatedAt (as shown above)

---

## ?? Troubleshooting

### Issue: "Database update failed"
```
Solution: 
1. Check database connectivity
2. Verify SQL Server is running
3. Check connection string in appsettings.json
4. Ensure you have permissions to modify the database
```

### Issue: "Unique key violation"
```
Solution:
1. Check if migration already applied
2. Run: dotnet ef migrations list
3. Verify target database is correct
```

### Issue: "Timeout during migration"
```
Solution:
1. If tables have millions of rows, migration may take time
2. Increase command timeout:
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       optionsBuilder.CommandTimeout(300); // 5 minutes
   }
```

### Issue: "Previous migration not found"
```
Solution:
1. Ensure you're in correct directory
2. Check Migrations folder exists
3. Run: dotnet ef migrations list (to see available migrations)
```

---

## ?? Questions?

If you encounter issues during migration:

1. Check the generated migration file: `Migrations/[Timestamp]_AddAuditTrailToBaseEntity.cs`
2. Review Entity Framework Core docs: https://docs.microsoft.com/ef/core/
3. Check SmartHRContext.cs for configuration issues
4. Verify database backups before applying migrations to production

---

## ?? Production Deployment Notes

**Important for production environments:**

1. **Backup First:** Always backup production database before migrations
2. **Test First:** Test migration on staging environment first
3. **Monitor Performance:** Large tables may take time to add columns
4. **Schedule Maintenance:** Plan migration during off-hours
5. **Keep Rollback Ready:** Save previous migration name for quick rollback
6. **Monitor Application:** Watch for errors after deployment

---

## ?? Performance Considerations

For large tables, consider:

```sql
-- Create index on UpdatedAt for faster queries
CREATE INDEX IX_Employees_UpdatedAt ON Employees(UpdatedAt DESC);

-- Create index on IsDeleted for soft-delete queries
CREATE INDEX IX_Employees_IsDeleted ON Employees(IsDeleted);

-- Composite index for common queries
CREATE INDEX IX_Employees_IsDeleted_UpdatedAt 
  ON Employees(IsDeleted, UpdatedAt DESC);
```

---

**Migration Guide Version:** 1.0
**Last Updated:** 2024
**Status:** Ready to Deploy
