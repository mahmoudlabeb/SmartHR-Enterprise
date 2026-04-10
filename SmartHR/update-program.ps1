$file = 'Program.cs'
$content = Get-Content $file -Raw
$newContent = $content -replace 'app\.Run\(\);', "RecurringJob.AddOrUpdate<PayrollJobService>("monthly-payroll", x => x.GenerateMonthlySalariesAsync(), "0 0 28 * *");
            app.Run();"
Set-Content $file $newContent
