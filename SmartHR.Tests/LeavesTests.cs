using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartHR.Controllers;
using SmartHR.Data;
using SmartHR.Hubs;
using SmartHR.Models;
using Xunit;

namespace SmartHR.Tests
{
    public class LeavesTests
    {
        private SmartHRContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SmartHRContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new SmartHRContext(options);
        }

        [Fact]
        public async Task Create_ShouldReject_OverlappingLeave()
        {
            using var context = CreateContext("overlap1");
            // seed employee and existing leave
            var emp = new Employee { Id = 1, FullName = "Alice", UserId = "u1" };
            context.Employees.Add(emp);
            context.Leaves.Add(new Leave
            {
                Id = 10,
                EmployeeId = 1,
                StartDate = new System.DateTime(2025, 1, 10),
                EndDate = new System.DateTime(2025, 1, 20),
                Status = LeaveStatus.Approved
            });
            await context.SaveChangesAsync();

            var hubMock = new Mock<IHubContext<NotificationHub>>();
            var userMgrMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var controller = new LeavesController(context, hubMock.Object, userMgrMock.Object);

            var newLeave = new Leave
            {
                EmployeeId = 1,
                StartDate = new System.DateTime(2025, 1, 15),
                EndDate = new System.DateTime(2025, 1, 25),
                LeaveType = LeaveType.Casual
            };

            var result = await controller.Create(newLeave);

            // Expect ViewResult due to overlap error
            Assert.IsType<Microsoft.AspNetCore.Mvc.ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Approve_ShouldSet_ApprovedByAndTimestamp_AndCreateAudit()
        {
            using var context = CreateContext("approve1");
            // seed employee requester and approver (users)
            var requesterEmployee = new Employee { Id = 2, FullName = "Bob", UserId = "req1" };
            var approverEmployee = new Employee { Id = 3, FullName = "Manager", UserId = "app1" };
            context.Employees.AddRange(requesterEmployee, approverEmployee);
            var leave = new Leave
            {
                Id = 200,
                EmployeeId = requesterEmployee.Id,
                StartDate = System.DateTime.Today.AddDays(1),
                EndDate = System.DateTime.Today.AddDays(2),
                Status = LeaveStatus.Pending
            };
            context.Leaves.Add(leave);
            await context.SaveChangesAsync();

            var hubMock = new Mock<IHubContext<NotificationHub>>();
            var clientsUsersMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsUsersMock.Object);
            clientsUsersMock.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxyMock.Object);

            var userMgrMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            var controller = new LeavesController(context, hubMock.Object, userMgrMock.Object);

            // set http context user to approver user id
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "app1") };
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
            };

            var result = await controller.Approve(leave.Id);

            // check redirect
            Assert.IsType<Microsoft.AspNetCore.Mvc.RedirectToActionResult>(result);

            var updated = await context.Leaves.FindAsync(leave.Id);
            Assert.Equal(LeaveStatus.Approved, updated.Status);
            Assert.Equal(approverEmployee.Id, updated.ApprovedByEmployeeId);
            Assert.NotNull(updated.ApprovedAt);

            var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.PrimaryKeyValue == leave.Id.ToString() && a.ActionType == "Approve");
            Assert.NotNull(audit);
            Assert.Equal("Leaves", audit.TableName);
        }
    }
}
