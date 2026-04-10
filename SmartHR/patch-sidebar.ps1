$p = 'Views/Shared/_PartialSidebar.cshtml'
$c = Get-Content $p -Raw
$new = @"
        <h6 class="sidebar-heading px-3 mt-3 mb-1 text-uppercase fw-bold small">التواصل الداخلي</h6>
        <ul class="nav flex-column mb-3 px-2">
            <li class="nav-item">
                <a class="nav-link" asp-controller="Chat" asp-action="Index">
                    <i class="fas fa-comments me-2 text-success w-20px text-center"></i> الرسائل والم�ادثات
                </a>
            </li>
        </ul>

        @if (User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager))
        {
            <h6 class="sidebar-heading px-3 mt-3 mb-1 text-uppercase fw-bold small">التقارير والإ�صائيات</h6>
            <ul class="nav flex-column mb-3 px-2">
                <li class="nav-item">
                    <a class="nav-link" asp-controller="Reports" asp-action="PnLReport">
                        <i class="fas fa-chart-line me-2 text-success w-20px text-center"></i> تقرير الأربا� والخسائر
                    </a>
                </li>
            </ul>
        }

        @if (User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.IT))
        {
            <h6 class="sidebar-heading px-3 mt-3 mb-1 text-uppercase fw-bold small">إدارة النظام</h6>
            <ul class="nav flex-column mb-4 px-2">
                <li class="nav-item">
                    <a class="nav-link" asp-controller="Roles" asp-action="Index">
                        <i class="fas fa-user-shield me-2 text-muted w-20px text-center"></i> الصلا�يات والأدوار
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" asp-controller="Users" asp-action="Pending">
                        <i class="fas fa-user-check me-2 text-warning w-20px text-center"></i> طلبات التسجيل
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" asp-controller="AuditLogs" asp-action="Index">
                        <i class="fas fa-history me-2 text-danger w-20px text-center"></i> سجل النظام
                    </a>
                </li>
            </ul>
        }
