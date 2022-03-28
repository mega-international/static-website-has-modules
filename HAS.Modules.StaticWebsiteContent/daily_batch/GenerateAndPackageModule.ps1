$scriptFolder = (get-item $PSScriptRoot ).FullName
#$instanceFolder = (get-item $PSScriptRoot ).Parent.Parent.Parent.FullName
#$moduleShadowFolder = (get-item $PSScriptRoot ).FullName
#$wwwrootFolder = "$moduleShadowFolder\wwwroot"
#$wwwrootTemplateFolder = "$moduleShadowFolder\wwwrootTemplate"



# Get variables from the config.json file in the same folder than the script
# .websiteId
# .apiKey
# .serverUrl
# .languagesCode
# .tempRefreshApi
# .forceContinuOnError
$jsonConfig = Get-Content "$scriptFolder\config.json" | Select -Skip 7  | Out-String | ConvertFrom-Json

#New-Item -ItemType Directory -Force -Path "$websitePath"
#Remove-Item $websitePath\* -Recurse -Force

$serverUrl = $jsonConfig.serverUrl
$uri = "$serverUrl/HOPEXGraphQL/api/async/generate-package-website"

$headers = @{
    'x-api-key' = $jsonConfig.apiKey
    'Content-Type' = 'application/json'
    'Accept' = '*/*'
    'Accept-Encoding' = 'gzip, deflate, br'
    'x-hopex-wait' = '1'
   }
$body = @{
	'webSiteId' = $jsonConfig.webSiteId
	'languagesCode' = $jsonConfig.languagesCode
    'forceContinuOnError' = $jsonConfig.forceContinuOnError
}| ConvertTo-Json
$response = Invoke-WebRequest -Uri $uri -Method Post -Headers $headers -Body $body
## For status check of the generated website and module deployment 
##		-> uncomment the code bellow
##		-> add complementary script to exploit the http results/ status code
# $responseConent = $response.Content
# if($responseConent -like '*:500*')
# {
# echo $response.Content
# Read-Host -Prompt "Press Enter to exit"
# Exit
# }
# $task = $response.Headers.'x-hopex-task'
# if($task -eq $null -or $task -eq "")
# {
# echo "GraphQL endpoint not reached. Please check version of GraphQL"
# Read-Host -Prompt "Press Enter to exit"
# Exit
# }
# $headers += @{'x-hopex-task' = $response.Headers.'x-hopex-task'}
# while($response.StatusCode -eq "206")
# {
# $response = Invoke-WebRequest -Uri $uri -Method Post -Headers $headers -Body $body 
# $responseConent = $response.Content
# if($responseConent -like '*:500*')
# {
# echo $response.Content
# Read-Host -Prompt "Press Enter to exit"
# Exit
# }
#$tempInMiliSecondes = $jsonConfig.tempRefreshApi * 1000
# echo "Waiting to refresh api async call"
# Start-Sleep -Milliseconds $tempInMiliSecondes
# }
# echo $response.Content



