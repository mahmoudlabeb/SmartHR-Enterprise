$file = 'Data\SmartHRContext.cs'
$content = Get-Content $file -Raw
$methodText = @"
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChanges(auditEntries);
            return result;
        }

        private System.Collections.Generic.List<AuditEntry> OnBeforeSaveChanges()
        {
            var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "System";
            ChangeTracker.DetectChanges();
            var auditEntries = new System.Collections.Generic.List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                // For simple audit we just log basic entity changes (Insert/Update/Delete)
                // A complete audit log would track every property modification.
                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    UserId = userId
                };
                auditEntries.Add(auditEntry);

                // Set BaseEntity timestamps
                if (entry.Entity is BaseEntity baseEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        baseEntity.CreatedAt = DateTime.UtcNow;
                        baseEntity.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        baseEntity.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // Save the logs for entries that don't need temporary IDs
            foreach (var auditEntry in auditEntries.Where(x => !x.HasTemporaryProperties))
            {
                AuditLogs.Add(auditEntry.ToAudit());
            }

            return auditEntries.Where(x => x.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChanges(System.Collections.Generic.List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                // Retrieve IDs from the temporary properties
                AuditLogs.Add(auditEntry.ToAudit());
            }

            return base.SaveChangesAsync();
        }
    }

    public class AuditEntry
    {
        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
        public string TableName { get; set; } = null!;
        public string? UserId { get; set; }
        public System.Collections.Generic.Dictionary<string, object?> KeyValues { get; } = new();
        public System.Collections.Generic.Dictionary<string, object?> OldValues { get; } = new();
        public System.Collections.Generic.Dictionary<string, object?> NewValues { get; } = new();
        public System.Collections.Generic.Dictionary<string, object?> ChangedProperties { get; } = new();
        public Microsoft.EntityFrameworkCore.EntityState State { get; set; }
        public System.Collections.Generic.List<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> TemporaryProperties { get; } = new();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            Entry = entry;
            State = entry.State;
            SetChanges();
        }

        private void SetChanges()
        {
            foreach (var property in Entry.Properties)
            {
                if (property.IsTemporary)
                {
                    TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (State)
                {
                    case Microsoft.EntityFrameworkCore.EntityState.Added:
                        NewValues[propertyName] = property.CurrentValue;
                        ChangedProperties[propertyName] = property.CurrentValue;
                        break;
                    case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                        OldValues[propertyName] = property.OriginalValue;
                        ChangedProperties[propertyName] = property.OriginalValue;
                        break;
                    case Microsoft.EntityFrameworkCore.EntityState.Modified:
                        if (property.IsModified)
                        {
                            OldValues[propertyName] = property.OriginalValue;
                            NewValues[propertyName] = property.CurrentValue;
                            ChangedProperties[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        public AuditLog ToAudit()
        {
            var audit = new AuditLog
            {
                TableName = TableName,
                ChangedByUserId = UserId,
                ChangedAt = DateTime.UtcNow,
                PrimaryKeyField = string.Join(",", KeyValues.Keys),
                PrimaryKeyValue = string.Join(",", KeyValues.Values),
                ActionType = State.ToString(),
                OldValue = OldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(OldValues),
                NewValue = NewValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(NewValues),
                PropertyName = ChangedProperties.Count == 0 ? null : string.Join(",", ChangedProperties.Keys)
            };
            return audit;
        }
