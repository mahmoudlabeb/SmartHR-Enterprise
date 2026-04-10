$p = 'Models/Ticket.cs'
$c = Get-Content $p -Raw
$c = $c -replace 'public DateTime CreatedAt \{ get; set; \} = DateTime\.Now;', "public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? AttachmentUrl { get; set; }"
Set-Content $p $c
