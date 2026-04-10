using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartHR.Models;

namespace SmartHR.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<SmartHRContext>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            // 1. Setup Roles
            string[] roles = { AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.IT, AppRoles.HR, AppRoles.Manager, AppRoles.Employee, AppRoles.Client };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2. Clear previous generic Data if we want (We won't delete, just ensure we add our rich Arabic data)
            // But checking for 'IT & Engineering' to see if we seeded the old English ones could be useful.

            // 3. Departments (Arabic)
            var deptsConfig = new List<(string Name, string Desc)>
            {
                ("إدارة تقنية المعلومات", "مسؤولة عن تطوير النظام والبنية التحتية"),
                ("إدارة الموارد البشرية", "شؤون الموظفين، التوظيف، والتطوير"),
                ("الإدارة المالية", "الرواتب، الحسابات، والتقارير المالية"),
                ("المبيعات والتسويق", "إدارة علاقات العملاء والحملات التسويقية"),
                ("الإدارة التنفيذية", "التخطيط الاستراتيجي والقيادة")
            };

            foreach (var dept in deptsConfig)
            {
                if (!await context.Departments.AnyAsync(d => d.Name == dept.Name))
                {
                    context.Departments.Add(new Department { Name = dept.Name, Description = dept.Desc });
                }
            }
            await context.SaveChangesAsync();

            // 4. Designations (Arabic) mapped to Departments
            var itDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "إدارة تقنية المعلومات");
            var hrDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "إدارة الموارد البشرية");
            var finDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "الإدارة المالية");
            var salesDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "المبيعات والتسويق");
            var execDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "الإدارة التنفيذية");

            var designationsMap = new List<(Department? Dept, List<string> Titles)>
            {
                (itDept, new List<string> { "مطور برمجيات مسؤول", "مهندس شبكات", "محلل نظم", "مطور واجهات أمامية" }),
                (hrDept, new List<string> { "مدير الموارد البشرية", "أخصائي توظيف", "مشرف شؤون الموظفين" }),
                (finDept, new List<string> { "مدير مالي", "محاسب أول", "مدقق داخلي" }),
                (salesDept, new List<string> { "مدير المبيعات", "مندوب مبيعات", "أخصائي تسويق رقمي" }),
                (execDept, new List<string> { "المدير التنفيذي (CEO)", "مدير العمليات (COO)" })
            };

            foreach (var mapping in designationsMap)
            {
                if (mapping.Dept != null)
                {
                    foreach (var title in mapping.Titles)
                    {
                        if (!await context.Designations.AnyAsync(d => d.Title == title))
                        {
                            context.Designations.Add(new Designation { Title = title, DepartmentId = mapping.Dept.Id });
                        }
                    }
                }
            }
            await context.SaveChangesAsync();

            // 5. Rich Employee Default Accounts
            var accountsConfig = new List<(string Email, string Pass, string Role, string Name, string? Title)>
            {
                ("superadmin@smarthr.com", "Admin@123", AppRoles.SuperAdmin, "أحمد الإداري (إدارة النظام)", "المدير التنفيذي (CEO)"),
                ("admin@smarthr.com", "Admin@123", AppRoles.Admin, "محمد مدير النظام", "محلل نظم"),
                ("it@smarthr.com", "ItAdmin@123!", AppRoles.IT, "مسؤول تقنية المعلومات", "محلل نظم"),
                ("hr@smarthr.com", "HrAdmin@123", AppRoles.HR, "طارق يوسف (مدير الموارد)", "مدير الموارد البشرية"),
                ("manager@smarthr.com", "Manager@123!", AppRoles.Manager, "فاطمة علي (المدير المالي)", "مدير مالي"),
                ("employee@smarthr.com", "Emp@123!", AppRoles.Employee, "خالد عبد الرحمن (مهندس)", "مطور برمجيات مسؤول"),
                ("employee2@smarthr.com", "Emp@123!", AppRoles.Employee, "نورة سلمان (محاسب)", "محاسب أول"),
                ("employee3@smarthr.com", "Emp@123!", AppRoles.Employee, "عمر حسن (تسويق)", "أخصائي تسويق رقمي"),
                ("client@smarthr.com", "Client@123!", AppRoles.Client, "شركة الأفق المحدودة", null)
            };

            var random = new Random();

            foreach (var acc in accountsConfig)
            {
                var user = await userManager.FindByEmailAsync(acc.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = acc.Email,
                        Email = acc.Email,
                        EmailConfirmed = true,
                        FullName = acc.Name,
                        IsActive = true,
                        HireDate = DateTime.UtcNow.AddDays(-random.Next(100, 1500))
                    };
                    var result = await userManager.CreateAsync(user, acc.Pass);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, acc.Role);
                    }
                    else
                    {
                        logger.LogError("User creation failed for {Email}: {Errors}", acc.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                        continue; // Skip the rest of the loop for this user
                    }
                }
                else
                {
                    // Force password reset to ensure it matches documentation
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    await userManager.ResetPasswordAsync(user, token, acc.Pass);
                    
                    // Also ensure they are active
                    if (!user.IsActive)
                    {
                        user.IsActive = true;
                        await userManager.UpdateAsync(user);
                    }
                }

                // If role is not Client and has Title, assign to Employee table
                if (acc.Title != null && !await context.Employees.AnyAsync(e => e.UserId == user.Id))
                {
                    var desig = await context.Designations.FirstOrDefaultAsync(d => d.Title == acc.Title);
                    if (desig != null)
                    {
                        var salary = random.Next(4000, 15000);
                        context.Employees.Add(new Employee
                        {
                            UserId = user.Id,
                            FullName = user.FullName ?? "",
                            Email = user.Email ?? "",
                            PhoneNumber = "05" + random.Next(10000000, 99999999).ToString(),
                            HireDate = user.HireDate,
                            BasicSalary = salary,
                            IsActive = true,
                            DepartmentId = desig.DepartmentId,
                            DesignationId = desig.Id
                        });
                    }
                }
            }
            await context.SaveChangesAsync();

            // 6. Generate Some Random Leave Requests
            var employees = await context.Employees.Take(5).ToListAsync();
            if (employees.Any() && !await context.Leaves.AnyAsync())
            {
                foreach (var emp in employees)
                {
                    context.Leaves.Add(new Leave
                    {
                        EmployeeId = emp.Id,
                        LeaveType = "سنوية",
                        StartDate = DateTime.UtcNow.AddDays(random.Next(5, 20)),
                        EndDate = DateTime.UtcNow.AddDays(random.Next(21, 30)),
                        Reason = "إجازة الموظف السنوية المعتمدة",
                        Status = random.Next(0, 2) == 0 ? "Pending" : "Approved"
                    });
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
