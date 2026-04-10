# SmartHR - Quick Reference Guide for Updated ViewModels

## Overview
This guide provides developers with quick reference information for the improved ViewModels with enhanced validation and type safety.

---

## ?? Permission System - Using ApplicationPermission Enum

### ? Modern Way (Type-Safe)
```csharp
using SmartHR.ViewModels;

// In Controllers or Services
var permissions = new[] { 
    ApplicationPermission.Employees,
    ApplicationPermission.Projects,
    ApplicationPermission.Salary
};

// Checking permissions
if (userPermissions.Contains(ApplicationPermission.Employees))
{
    // Grant access
}

// In Views
@if (userRole.HasPermission(ApplicationPermission.Reports))
{
    <a href="@Url.Action("Index", "Reports")">View Reports</a>
}
```

### ? Old Way (String-Based - Avoid)
```csharp
// Not recommended anymore
if (permission == "Employees")
{
    // Easy to typo: "Employes" or "Employees " with space
}
```

### Available Permissions
```csharp
ApplicationPermission.Employees        // Employee management
ApplicationPermission.Departments      // Department management
ApplicationPermission.Projects         // Project management
ApplicationPermission.LeaveAttendance  // Leave & Attendance
ApplicationPermission.Salary           // Payroll & Salary
ApplicationPermission.Finance          // Invoices & Expenses
ApplicationPermission.Recruitment      // Recruitment & Candidates
ApplicationPermission.Training         // Training Programs
ApplicationPermission.Tickets          // Support Tickets
ApplicationPermission.Reports          // Reports & Analytics
ApplicationPermission.Settings         // System Settings
ApplicationPermission.Admin            // Administration
```

---

## ?? Report ViewModels - Validation Examples

### EmployeeReportViewModel
```csharp
var reportModel = new EmployeeReportViewModel
{
    DepartmentFilter = "Engineering",
    StatusFilter = "Active",
    Employees = employeeList
};

// Validation automatically applied via DataAnnotations
if (!ModelState.IsValid)
{
    // Return errors to view
    return View(reportModel);
}
```

### SalaryReportViewModel
```csharp
var salaryReport = new SalaryReportViewModel
{
    Month = 12,    // Range: 1-12 (enforced)
    Year = 2024,   // Range: 2000-2100 (enforced)
    TotalPayroll = 500000.00m,
    Salaries = salaryItems
};

// Month must be 1-12, Year must be 2000-2100
// Validation happens automatically
```

### ProjectReportViewModel
```csharp
foreach (var project in projects)
{
    // Progress must be 0-100
    var item = new ProjectReportItem
    {
        ProjectName = project.Name,
        ClientName = project.Client.Name,
        Progress = 85,  // Validated to be 0-100
        EndDate = project.EndDate,
        Status = "Active"
    };
}
```

---

## ?? Settings ViewModels - Validation Rules

### CompanyInfoViewModel
```csharp
var settings = new CompanyInfoViewModel
{
    CompanyName = "My Company",              // Required, 3-300 chars
    TaxNumber = "1234567890",                // Required, digits only, 5-50 chars
    PhoneNumber = "+966-12-3456789",         // Required, valid phone format
    Email = "info@company.com",              // Required, valid email
    Address = "123 Main St, City, Country"   // Required, 10-500 chars
};
```

### LocalizationViewModel
```csharp
var localization = new LocalizationViewModel
{
    Language = "en",                   // Valid locale: ar, en, fr, etc.
    TimeZone = "Asia/Riyadh",         // Valid timezone string
    DateFormat = "dd/MM/yyyy"         // Valid format pattern
};
```

### ThemeViewModel
```csharp
var theme = new ThemeViewModel
{
    ThemeMode = "Dark",               // Must be: Light, Dark, or System
    PrimaryColor = "#0d6efd"          // Must be valid hex: #RRGGBB
};

// Validation examples
// ? Valid: "#0d6efd", "#FF0000", "#abc123"
// ? Invalid: "0d6efd", "#0d6ef", "blue"
```

---

## ?? Authentication ViewModels

### LoginViewModel
```csharp
var login = new LoginViewModel
{
    Email = "user@company.com",     // Required, valid email
    Password = "SecurePass123",     // Required, max 128 chars
    RememberMe = true
};
```

### ForgotPasswordViewModel
```csharp
var forgot = new ForgotPasswordViewModel
{
    Email = "user@company.com"      // Required, valid email format
};
```

### ResetPasswordViewModel
```csharp
var reset = new ResetPasswordViewModel
{
    Email = "user@company.com",     // Required, valid email
    Password = "NewPass123",        // Required, 8+ chars, mixed case + digits
    ConfirmPassword = "NewPass123", // Must match Password
    Code = "reset_token_here"       // Required (from email link)
};

// Password rules:
// ? Must have: uppercase, lowercase, and digits
// ? Min length: 8 characters
// Example: "MyPassword123" ?
// Example: "password123" ? (no uppercase)
```

---

## ??? Validation in Controllers

### Server-Side Validation
```csharp
[HttpPost]
public async Task<IActionResult> CreateReport(SalaryReportViewModel model)
{
    // Validation happens automatically
    if (!ModelState.IsValid)
    {
        // Return model with errors
        return View(model);
    }
    
    // Model is guaranteed to be valid here
    // Month is 1-12, Year is 2000-2100, etc.
    var report = await _service.GenerateReportAsync(model);
    return View("Report", report);
}
```

### Getting Validation Errors
```csharp
if (!ModelState.IsValid)
{
    var errors = ModelState
        .Where(x => x.Value.Errors.Count > 0)
        .ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
        );
    
    return Json(new { success = false, errors });
}
```

---

## ?? Validation in Views

### Client-Side Validation (Razor)
```html
@model LoginViewModel

<form asp-action="Login" method="post">
    <div class="form-group">
        <label asp-for="Email"></label>
        <input asp-for="Email" class="form-control" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="Password"></label>
        <input asp-for="Password" class="form-control" />
        <span asp-validation-for="Password" class="text-danger"></span>
    </div>
</form>

@section Scripts {
    @await Html.PartialAsync("_ValidationScriptsPartial")
}
```

### Display Names in Views
```html
<!-- Display attribute automatically generates labels -->
<!-- [Display(Name = "Email Address")] -->
<label asp-for="Email"></label>
<!-- Renders as: <label for="Email">Email Address</label> -->
```

---

## ??? DateTime Best Practices

### ? Correct Usage (UTC)
```csharp
// Store in database as UTC
var entity = new Employee
{
    CreatedAt = DateTime.UtcNow,      // UTC ?
    UpdatedAt = DateTime.UtcNow,      // UTC ?
    HireDate = DateTime.UtcNow.Date   // UTC ?
};

// Display to user (convert to local)
var localTime = entity.CreatedAt.ToLocalTime();
```

### ? Incorrect Usage (Local Time)
```csharp
// Don't use local time for storage
var entity = new Employee
{
    CreatedAt = DateTime.Now,  // ? Local time issues
};
```

### In Views
```csharp
// Convert UTC to user's timezone in views
@{
    var localTime = Model.CreatedAt.ToLocalTime();
}
<p>Created: @localTime.ToString("dd/MM/yyyy HH:mm")</p>
```

---

## ?? Audit Trail - Using UpdatedAt

### Tracking Changes
```csharp
public async Task<IActionResult> Edit(int id, EmployeeViewModel model)
{
    var employee = await _context.Employees.FindAsync(id);
    
    if (employee != null)
    {
        employee.FullName = model.FullName;
        employee.Email = model.Email;
        employee.UpdatedAt = DateTime.UtcNow;  // Track modification
        
        _context.Update(employee);
        await _context.SaveChangesAsync();
    }
    
    return RedirectToAction("Index");
}
```

### Querying by Update Time
```csharp
// Find recently updated employees
var recentlyUpdated = await _context.Employees
    .Where(e => e.UpdatedAt > DateTime.UtcNow.AddDays(-7))
    .ToListAsync();

// Find records that haven't changed in a month
var stale = await _context.Employees
    .Where(e => e.UpdatedAt < DateTime.UtcNow.AddMonths(-1))
    .ToListAsync();
```

---

## ?? Testing Examples

### Unit Test with Validation
```csharp
[Test]
public void SalaryReportViewModel_MonthValidation()
{
    var model = new SalaryReportViewModel
    {
        Month = 13,  // Invalid: out of range
        Year = 2024
    };
    
    var context = new ValidationContext(model);
    var results = new List<ValidationResult>();
    
    var isValid = Validator.TryValidateObject(
        model, context, results, true);
    
    Assert.IsFalse(isValid);
    Assert.IsTrue(results.Any(r => r.MemberNames.Contains("Month")));
}
```

---

## ?? Migration Guide

### Old Code to New Code

#### Permission Checking
```csharp
// OLD
if (permission == "Employees" || permission == "employees")
{
    // Access granted
}

// NEW
if (userPermissions.Contains(ApplicationPermission.Employees))
{
    // Access granted - Type-safe!
}
```

#### DateTime Handling
```csharp
// OLD
var entity = new Employee { CreatedAt = DateTime.Now };

// NEW
var entity = new Employee { CreatedAt = DateTime.UtcNow };
```

#### Validation
```csharp
// OLD - No validation
public class ReportViewModel
{
    public int Month { get; set; }
    public int Year { get; set; }
}

// NEW - Comprehensive validation
public class SalaryReportViewModel
{
    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
    public int Month { get; set; }
    
    [Range(2000, 2100)]
    public int Year { get; set; }
}
```

---

## ?? Support & Documentation

- **XML Documentation:** Hover over properties in VS for detailed docs
- **DataAnnotations Docs:** https://docs.microsoft.com/dotnet/api/system.componentmodel.dataannotations
- **Entity Framework:** https://docs.microsoft.com/ef/core/

---

## ? Checklist for New ViewModels

When creating new ViewModels, follow this checklist:

- [ ] Add XML documentation to class
- [ ] Add XML documentation to each property
- [ ] Add `[Required]` attribute where needed
- [ ] Add `[StringLength]` for string properties
- [ ] Add `[Range]` for numeric properties
- [ ] Add `[EmailAddress]` for email fields
- [ ] Add `[Phone]` for phone fields
- [ ] Add `[RegularExpression]` for special formats
- [ ] Add `[Display(Name = "...")]` for UI labels
- [ ] Add meaningful error messages
- [ ] Use `DateTime.UtcNow` for timestamps
- [ ] Initialize collections with `new List<T>()`
- [ ] Initialize strings with `string.Empty`

---

**Last Updated:** 2024
**Version:** 1.0
