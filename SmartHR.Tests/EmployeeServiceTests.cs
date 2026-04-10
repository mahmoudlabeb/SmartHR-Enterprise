using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Services;
using SmartHR.ViewModels;
using Xunit;

namespace SmartHR.Tests
{
    public class EmployeeServiceTests
    {
        private SmartHRContext GetContext()
        {
            var options = new DbContextOptionsBuilder<SmartHRContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new SmartHRContext(options);
        }

        [Fact]
        public async Task CreateEmployeeAsync_AddsEmployeeToDatabase()
        {
            // Arrange
            var context = GetContext();
            var service = new EmployeeService(context);
            var model = new EmployeeCreateViewModel
            {
                FullName = "Test User",
                Email = "test@company.com",
                PhoneNumber = "1234567890",
                BasicSalary = 5000,
                IsActive = true,
                DepartmentId = 1,
                DesignationId = 1
            };

            // Act
            await service.CreateEmployeeAsync(model);

            // Assert
            var count = await context.Employees.CountAsync();
            Assert.Equal(1, count);
            var employee = await context.Employees.FirstAsync();
            Assert.Equal("Test User", employee.FullName);
            Assert.Equal("test@company.com", employee.Email);
        }

        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsAllActiveEmployees()
        {
            // Arrange
            var context = GetContext();
            var dept = new Department { Id = 1, Name = "HR" };
            var desig = new Designation { Id = 1, Title = "Mgr" };
            context.Departments.Add(dept);
            context.Designations.Add(desig);

            context.Employees.Add(new Employee { FullName = "Active User", Email = "u1@b.com", IsDeleted = false, DepartmentId = 1, DesignationId = 1 });
            context.Employees.Add(new Employee { FullName = "Deleted User", Email = "u2@b.com", IsDeleted = true, DepartmentId = 1, DesignationId = 1 });
            await context.SaveChangesAsync();

            var service = new EmployeeService(context);

            // Act
            var result = await service.GetAllEmployeesAsync(null);

            // Assert
            Assert.Single(result);
            Assert.Equal("Active User", result.First().FullName);
        }

        [Fact]
        public async Task SoftDeleteEmployeeAsync_SetsIsDeletedFlag()
        {
            // Arrange
            var context = GetContext();
            var emp = new Employee { FullName = "Test", Email = "test@b.com", IsDeleted = false };
            context.Employees.Add(emp);
            await context.SaveChangesAsync();

            var service = new EmployeeService(context);

            // Act
            await service.SoftDeleteEmployeeAsync(emp.Id);

            // Assert
            var deletedEmp = await context.Employees.IgnoreQueryFilters().FirstAsync();
            Assert.True(deletedEmp.IsDeleted);
        }
    }
}
