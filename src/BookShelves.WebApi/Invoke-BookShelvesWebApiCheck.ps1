[CmdletBinding()]
param(
	[Parameter(Mandatory = $true)]
	[string]$BaseUrl,

	[string]$Path = "/api/Test",

	[string]$TenantId,

	[string]$ClientId,

	[string]$ClientSecret,

	[string]$Scope = "api://a98249d2-b51b-41d6-9c2a-5dadf7cf276f/.default",

	[int[]]$ExpectedStatusCodes,

	[int]$TimeoutSec = 30,

	[switch]$SkipToken
)

<#!
.SYNOPSIS
Quick CLI validation for the BookShelves Web API.

.DESCRIPTION
Calls a BookShelves Web API endpoint directly or through the nginx front door.
Use -SkipToken for a quick reachability check that expects a 401 on protected routes,
or provide Entra ID client credentials to validate authentication and authorization.

.EXAMPLE
.\Invoke-BookShelvesWebApiCheck.ps1 -BaseUrl "https://bookshelves.azmoore.com" -SkipToken

.EXAMPLE
.\Invoke-BookShelvesWebApiCheck.ps1 -BaseUrl "https://bookshelves.azmoore.com" -TenantId "<tenant-id>" -ClientId "<client-id>" -ClientSecret "<client-secret>"

.EXAMPLE
.\Invoke-BookShelvesWebApiCheck.ps1 -BaseUrl "https://bookshelves.azmoore.com" -Path "/api/WeatherForecast" -TenantId "<tenant-id>" -ClientId "<client-id>" -ClientSecret "<client-secret>" -ExpectedStatusCodes 200

.EXAMPLE
.\Invoke-BookShelvesWebApiCheck.ps1 -BaseUrl "http://localhost:5001" -Path "/Test" -SkipToken -ExpectedStatusCodes 401
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-NormalizedUri {
	param(
		[Parameter(Mandatory = $true)]
		[string]$BaseUrl,

		[Parameter(Mandatory = $true)]
		[string]$Path
	)

	$normalizedBaseUrl = if ($BaseUrl.EndsWith('/')) { $BaseUrl } else { "$BaseUrl/" }
	$baseUri = [Uri]::new($normalizedBaseUrl)
	$relativePath = $Path.TrimStart('/')

	return [Uri]::new($baseUri, $relativePath)
}

function New-HttpClient {
	param(
		[Parameter(Mandatory = $true)]
		[int]$TimeoutSec
	)

	$handler = [System.Net.Http.HttpClientHandler]::new()
	$client = [System.Net.Http.HttpClient]::new($handler)
	$client.Timeout = [TimeSpan]::FromSeconds($TimeoutSec)
	$client.DefaultRequestHeaders.Accept.Clear()
	$client.DefaultRequestHeaders.Accept.Add([System.Net.Http.Headers.MediaTypeWithQualityHeaderValue]::new('application/json'))

	return $client
}

function Get-AccessToken {
	param(
		[Parameter(Mandatory = $true)]
		[string]$TenantId,

		[Parameter(Mandatory = $true)]
		[string]$ClientId,

		[Parameter(Mandatory = $true)]
		[string]$ClientSecret,

		[Parameter(Mandatory = $true)]
		[string]$Scope,

		[Parameter(Mandatory = $true)]
		[int]$TimeoutSec
	)

	$tokenUri = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"
	$client = New-HttpClient -TimeoutSec $TimeoutSec

	try {
		$formValues = New-Object 'System.Collections.Generic.Dictionary[string,string]'
		$formValues.Add('grant_type', 'client_credentials')
		$formValues.Add('client_id', $ClientId)
		$formValues.Add('client_secret', $ClientSecret)
		$formValues.Add('scope', $Scope)

		$content = [System.Net.Http.FormUrlEncodedContent]::new($formValues)
		$response = $client.PostAsync($tokenUri, $content).GetAwaiter().GetResult()
		$body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

		if (-not $response.IsSuccessStatusCode) {
			throw "Token request failed with status code $([int]$response.StatusCode) and body: $body"
		}

		$payload = $body | ConvertFrom-Json

		if ([string]::IsNullOrWhiteSpace($payload.access_token)) {
			throw 'Token request completed but no access_token was returned.'
		}

		return $payload.access_token
	}
	finally {
		$client.Dispose()
	}
}

function Invoke-GetRequest {
	param(
		[Parameter(Mandatory = $true)]
		[Uri]$Uri,

		[string]$BearerToken,

		[Parameter(Mandatory = $true)]
		[int]$TimeoutSec
	)

	$client = New-HttpClient -TimeoutSec $TimeoutSec

	try {
		if (-not [string]::IsNullOrWhiteSpace($BearerToken)) {
			$client.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $BearerToken)
		}

		$response = $client.GetAsync($Uri).GetAwaiter().GetResult()
		$body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

		return [pscustomobject]@{
			RequestUri         = $Uri.AbsoluteUri
			StatusCode         = [int]$response.StatusCode
			ReasonPhrase       = $response.ReasonPhrase
			IsSuccessStatusCode = $response.IsSuccessStatusCode
			Body               = $body
		}
	}
	finally {
		$client.Dispose()
	}
}

if (-not $SkipToken) {
	foreach ($requiredParameter in 'TenantId', 'ClientId', 'ClientSecret') {
		if ([string]::IsNullOrWhiteSpace((Get-Variable -Name $requiredParameter -ValueOnly))) {
			throw "-$requiredParameter is required unless -SkipToken is used."
		}
	}
}

if ($null -eq $ExpectedStatusCodes -or $ExpectedStatusCodes.Count -eq 0) {
	$ExpectedStatusCodes = if ($SkipToken) { @(401, 200) } else { @(200) }
}

$requestUri = Get-NormalizedUri -BaseUrl $BaseUrl -Path $Path
$accessToken = $null

if (-not $SkipToken) {
	$accessToken = Get-AccessToken -TenantId $TenantId -ClientId $ClientId -ClientSecret $ClientSecret -Scope $Scope -TimeoutSec $TimeoutSec
}

$result = Invoke-GetRequest -Uri $requestUri -BearerToken $accessToken -TimeoutSec $TimeoutSec

Write-Host "Request URI: $($result.RequestUri)"
Write-Host "Status Code: $($result.StatusCode) $($result.ReasonPhrase)"
Write-Host "Expected: $($ExpectedStatusCodes -join ', ')"

if (-not [string]::IsNullOrWhiteSpace($result.Body)) {
	Write-Host 'Response Body:'
	Write-Host $result.Body
}

$result

if ($ExpectedStatusCodes -notcontains $result.StatusCode) {
	throw "Unexpected status code $($result.StatusCode). Expected one of: $($ExpectedStatusCodes -join ', ')."
}
