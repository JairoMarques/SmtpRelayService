$SmtpServer = "192.168.64.103"
$SmtpFrom = "from@mydomain.com"
$SmtpTo = "destionation@domain.com"
$MessageSubject = "Test SMTP"
$MessageBody = "Email body"
$SmtpUser = "user@domain.com"
$SmtpPassword = "password"

Send-MailMessage -From $SmtpFrom -To $SmtpTo -Subject $MessageSubject -Body $MessageBody -SmtpServer $SmtpServer -Credential (New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $SmtpUser, (ConvertTo-SecureString $SmtpPassword -AsPlainText -Force)) -Port 25



