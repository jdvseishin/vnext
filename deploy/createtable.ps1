param(
    [string]$serverInstance,
    [string]$databaseName,
    [string]$username,
    [string]$pwd
)

Invoke-Sqlcmd -InputFile "deploy/CreateDeviceTable.sql" -ServerInstance $serverInstance -Database $databaseName -Username $username -Password $pwd -QueryTimeout 36000 -Verbose