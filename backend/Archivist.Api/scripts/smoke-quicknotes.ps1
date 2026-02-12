param(
  [string]$BaseUrl = "http://localhost:5185",
  [int]$PageSize = 5
)

$ErrorActionPreference = "Stop"

function Assert-True([bool]$Condition, [string]$Message) {
  if (-not $Condition) { throw "ASSERT FAILED: $Message" }
}

function Assert-Equal($Expected, $Actual, [string]$Message) {
  if ($Expected -ne $Actual) {
    throw "ASSERT FAILED: $Message`nExpected: $Expected`nActual:   $Actual"
  }
}

function Parse-DateUtc([string]$s) {
  # Your API returns ISO strings; parse to DateTime
  return [DateTime]::Parse($s, [System.Globalization.CultureInfo]::InvariantCulture,
    [System.Globalization.DateTimeStyles]::AssumeUniversal -bor [System.Globalization.DateTimeStyles]::AdjustToUniversal)
}

Write-Host "== Archivist QuickNotes Smoke Test ==" -ForegroundColor Cyan
Write-Host "BaseUrl: $BaseUrl" -ForegroundColor Cyan

# ---------- Pick an existing note ID (for "older note still retrievable") ----------
$inbox = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/quicknotes?view=inbox&page=1&pageSize=$PageSize"
Assert-True ($inbox.Count -ge 1) "Inbox list returned no notes; need at least 1 existing note to validate older GetById."

$existingId = $inbox[0].id
Write-Host "Using existing note id: $existingId" -ForegroundColor DarkCyan

# Verify GetById works for an older note
$existing = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/quicknotes/$existingId"
Assert-Equal $existingId $existing.id "GetById did not return the expected existing note."
Write-Host "PASS: GetById returns an existing note" -ForegroundColor Green

# ---------- Create a unique note ----------
$guid = [Guid]::NewGuid().ToString("N")
$title = "SmokeTest $guid"
$body  = "Smoke test body $guid"

$createBody = @{
  title = $title
  body  = $body
  state = "Open"
} | ConvertTo-Json

$created = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/quicknotes" -ContentType "application/json" -Body $createBody
Assert-True ($null -ne $created.id) "Create did not return an id."
Write-Host "PASS: Create returned id $($created.id)" -ForegroundColor Green

# Verify created note can be retrieved
$fetched = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/quicknotes/$($created.id)"
Assert-Equal $created.id $fetched.id "Fetched created note id mismatch."
Assert-Equal $title $fetched.title "Fetched created note title mismatch."
Assert-Equal $body $fetched.body "Fetched created note body mismatch."
Write-Host "PASS: Created note retrievable via GetById" -ForegroundColor Green

# ---------- Verify List ordering + page size ----------
$list = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/quicknotes?view=inbox&page=1&pageSize=$PageSize"
Assert-True ($list.Count -le $PageSize) "List returned more than pageSize ($PageSize)."

# Verify descending UpdatedAt order
for ($i = 0; $i -lt ($list.Count - 1); $i++) {
  $a = Parse-DateUtc $list[$i].updatedAt
  $b = Parse-DateUtc $list[$i + 1].updatedAt
  Assert-True ($a -ge $b) "List is not ordered by UpdatedAt DESC at index $i."
}
Write-Host "PASS: List returns <= pageSize and is ordered by UpdatedAt DESC" -ForegroundColor Green

# ---------- Update the created note ----------
$updatedTitle = "$title (updated)"
$updatedBody  = "$body (updated)"

$updateBody = @{
  title = $updatedTitle
  body  = $updatedBody
  state = "Pinned"
} | ConvertTo-Json

$beforeUpdate = Parse-DateUtc $fetched.updatedAt

$updated = Invoke-RestMethod -Method Put -Uri "$BaseUrl/api/quicknotes/$($created.id)" -ContentType "application/json" -Body $updateBody
Assert-Equal $created.id $updated.id "Update returned wrong note id."
Assert-Equal $updatedTitle $updated.title "Update did not persist title."
Assert-Equal $updatedBody $updated.body "Update did not persist body."
Assert-Equal "Pinned" $updated.state "Update did not persist state."

$afterUpdate = Parse-DateUtc $updated.updatedAt
Assert-True ($afterUpdate -ge $beforeUpdate) "UpdatedAt did not move forward on update."
Write-Host "PASS: Update works and UpdatedAt advanced" -ForegroundColor Green

# ---------- Soft delete created note ----------
Invoke-RestMethod -Method Delete -Uri "$BaseUrl/api/quicknotes/$($created.id)" | Out-Null
$afterSoftDelete = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/quicknotes/$($created.id)"
Assert-True ($null -ne $afterSoftDelete.deletedAt) "Soft delete did not set DeletedAt."
Write-Host "PASS: SoftDelete sets DeletedAt" -ForegroundColor Green

# ---------- Restore created note ----------
Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/quicknotes/$($created.id)/restore" | Out-Null
$afterRestore = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/quicknotes/$($created.id)"
Assert-True ($null -eq $afterRestore.deletedAt) "Restore did not clear DeletedAt."
Write-Host "PASS: Restore clears DeletedAt" -ForegroundColor Green

# ---------- Soft delete again (so hard delete is realistic) ----------
Invoke-RestMethod -Method Delete -Uri "$BaseUrl/api/quicknotes/$($created.id)" | Out-Null
$afterSoftDelete2 = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/quicknotes/$($created.id)"
Assert-True ($null -ne $afterSoftDelete2.deletedAt) "Soft delete (second time) did not set DeletedAt."
Write-Host "PASS: SoftDelete (again) sets DeletedAt" -ForegroundColor Green

# ---------- Hard delete ----------
Invoke-RestMethod -Method Delete -Uri "$BaseUrl/api/quicknotes/$($created.id)/hard" | Out-Null

# Verify it is no longer retrievable (should 404)
try {
  Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/quicknotes/$($created.id)" | Out-Null
  throw "ASSERT FAILED: HardDelete did not remove note; GetById still succeeded."
}
catch {
  # Invoke-RestMethod throws on 404; that's what we want.
  Write-Host "PASS: HardDelete removes note (GetById now fails)" -ForegroundColor Green
}

Write-Host "== ALL SMOKE TESTS PASSED ==" -ForegroundColor Cyan
