$p = 'Models/Message.cs'
$c = Get-Content $p -Raw
$c = $c -replace 'public bool IsRead \{ get; set; \}', "public bool IsRead { get; set; }

        public string? AttachmentUrl { get; set; }"
Set-Content $p $c
