using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartHR.Models;

namespace SmartHR.Data
{
    public class SmartHRContext : IdentityDbContext<ApplicationUser>
    {
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor? _httpContextAccessor;

        public SmartHRContext(DbContextOptions<SmartHRContext> options, Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor = null) : base(options) 
        { 
            _httpContextAccessor = httpContextAccessor;
        }

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

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    UserId = userId
                };
                auditEntries.Add(auditEntry);

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
                AuditLogs.Add(auditEntry.ToAudit());
            }

            return base.SaveChangesAsync();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            // Suppress the warning about required relationships with global query filters
            // (e.g., Employee -> Department with soft-delete filter).
            optionsBuilder.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
        }

        // ── 1. Organizational / Employees ────────────────────────────────
        public DbSet<Employee>    Employees    { get; set; }
        public DbSet<Department>  Departments  { get; set; }
        public DbSet<Designation> Designations { get; set; }

        // ── 2. Projects / Tasks ──────────────────────────────────────────
        public DbSet<Project>        Projects        { get; set; }
        public DbSet<ProjectMember>  ProjectMembers  { get; set; }
        public DbSet<TaskItem>       Tasks           { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

        // ── 3. Clients / Contacts ────────────────────────────────────────
        public DbSet<Client>  Clients  { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        // ── 4. Tickets / Support ─────────────────────────────────────────
        public DbSet<Ticket>        Tickets        { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }

        // ── 5. HR (Leaves / Attendance) ──────────────────────────────────
        public DbSet<Leave>      Leaves      { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        // ── 6. Finance (Salary / Invoices / Expenses) ────────────────────
        public DbSet<Salary>      Salaries     { get; set; }
        public DbSet<PaySlip>     PaySlips     { get; set; }
        public DbSet<Expense>     Expenses     { get; set; }
        public DbSet<Invoice>     Invoices     { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Message>     Messages     { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }

        // ── 7. Recruitment / Training ────────────────────────────────────
        public DbSet<Job>             Jobs             { get; set; }
        public DbSet<JobPosting>      JobPostings      { get; set; }
        public DbSet<Candidate>       Candidates       { get; set; }
        public DbSet<Training>        Trainings        { get; set; }
        public DbSet<Trainer>         Trainers         { get; set; }
        public DbSet<Trainee>         Trainees         { get; set; }
        public DbSet<TrainingProgram> TrainingPrograms { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Add index for quick lookup of approvals
            builder.Entity<Leave>().HasIndex(l => l.ApprovedByEmployeeId);

            // ── A. Composite primary keys (Many-to-Many join tables) ─────
            builder.Entity<ProjectMember>()
                .HasKey(pm => new { pm.ProjectId, pm.EmployeeId });

            builder.Entity<TaskAssignment>()
                .HasKey(ta => new { ta.TaskItemId, ta.EmployeeId });

            builder.Entity<Trainee>()
                .HasKey(tr => new { tr.TrainingId, tr.EmployeeId });

            // ── B. Employee ↔ AspNetUsers relationship (UserId as soft-FK) ─
            // ✅ FIX S2: Declare the relationship explicitly so EF knows about
            //    the link between Employee.UserId and ApplicationUser.Id.
            //    We use HasOne without a navigation property on ApplicationUser
            //    side to avoid circular dependencies.
            builder.Entity<Employee>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ── C. Disable cascade delete globally (prevents accidental loss) ─
            foreach (var relationship in builder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys())
                // Skip the Employee→User FK we just defined above (already SetNull)
                .Where(fk => fk.DeleteBehavior != DeleteBehavior.SetNull))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // ── D. Decimal precision for all financial columns ───────────
            foreach (var property in builder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(4);
            }

            // ── E. Soft-delete global query filters ──────────────────────
            // ✅ FIX M3: Entities that inherit from BaseEntity (which has an
            //    IsDeleted flag) now have a HasQueryFilter applied so that
            //    soft-deleted records are automatically excluded from all
            //    EF queries.  Use _context.Employees.IgnoreQueryFilters() when
            //    you specifically need to see deleted records (e.g., audit page).
            ApplySoftDeleteFilter<Employee>   (builder);
            ApplySoftDeleteFilter<Department> (builder);
            ApplySoftDeleteFilter<Designation>(builder);
            ApplySoftDeleteFilter<Leave>      (builder);
            ApplySoftDeleteFilter<Attendance> (builder);
            ApplySoftDeleteFilter<Project>    (builder);
            ApplySoftDeleteFilter<EmployeeDocument>(builder);
        }

        /// <summary>
        /// Adds a global EF query filter that automatically excludes rows
        /// where <see cref="BaseEntity.IsDeleted"/> is <c>true</c>.
        /// </summary>
        private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder)
            where TEntity : BaseEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
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
            var propNames = ChangedProperties.Count == 0 ? null : string.Join(",", ChangedProperties.Keys);
            var pkVal = string.Join(",", KeyValues.Values);
            var audit = new AuditLog
            {
                TableName = TableName,
                ChangedByUserId = UserId,
                ChangedAt = DateTime.UtcNow,
                PrimaryKeyField = string.Join(",", KeyValues.Keys),
                PrimaryKeyValue = pkVal.Length > 255 ? pkVal.Substring(0, 252) + "..." : pkVal,
                ActionType = State.ToString(),
                OldValue = OldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(OldValues),
                NewValue = NewValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(NewValues),
                PropertyName = propNames != null && propNames.Length > 255 ? propNames.Substring(0, 252) + "..." : propNames
            };
            return audit;
        }
    }
}