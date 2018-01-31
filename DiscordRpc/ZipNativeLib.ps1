$ifs = New-Object System.IO.FileStream "discord-rpc.dll", ([IO.FileMode]::Open), ([IO.FileAccess]::Read), ([IO.FileShare]::Read)
$ofs = New-Object System.IO.FileStream "discord-rpc.dll.gz", ([IO.FileMode]::Create), ([IO.FileAccess]::Write), ([IO.FileShare]::None)
$gzs = New-Object System.IO.Compression.GZipStream $ofs, ([IO.Compression.CompressionMode]::Compress)

$ifs.CopyTo($gzs);

$ifs.Close();
$gzs.Close();

Write-Host "Compressed discord-rpc.dll for embedding."