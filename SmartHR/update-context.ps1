$file = 'Data\SmartHRContext.cs'
$content = Get-Content $file -Raw
$newConstructor = @"
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor? _httpContextAccessor;

    public SmartHRContext(DbContextOptions<SmartHRContext> options, Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
