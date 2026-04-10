$p = 'Controllers/TicketsController.cs'
$c = Get-Content $p -Raw

$old = '        public async Task<IActionResult> Create\(Ticket ticket\)[\s\S]*?if \(ModelState\.IsValid\)\s*\{\s*ticket\.CreatedAt = DateTime\.Now;'

$new = @"
        public async Task<IActionResult> Create(Ticket ticket, IFormFile? attachment, [FromServices] IWebHostEnvironment env)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isPrivileged = User.IsInRole(AppRoles.SuperAdmin)
                               || User.IsInRole(AppRoles.Admin)
                               || User.IsInRole(AppRoles.IT)
                               || User.IsInRole(AppRoles.HR)
                               || User.IsInRole(AppRoles.Manager);

            if (!isPrivileged)
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == currentUserId);
                if (employee == null) 
                {
                    TempData["ErrorMessage"] = "Cannot create ticket, not linked to HR profile.";
                    return RedirectToAction("Index", "Dashboard");
                }
                ticket.EmployeeId = employee.Id;
            }

            if (ModelState.IsValid)
            {
                if (attachment != null && attachment.Length > 0)
                {
                    string uploadsFolder = Path.Combine(env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + attachment.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await attachment.CopyToAsync(fileStream);
                    }
                    ticket.AttachmentUrl = "/uploads/" + uniqueFileName;
                }

                ticket.CreatedAt = DateTime.Now;
