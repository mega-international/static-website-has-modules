#Has Static Web Site

## Prerequisits
Install the website.static.content module

##Website generation and module packaging by script
In the website.static.module you have a script folder containing :
    - config.txt
    - GenerateWebSite.vbs
    - GenerateAndPackageModule.ps1

First, open the /website.static.content/script/config.txt file and put the informations that corresponds to your website generation(s). 

For several languages:
    copy the content of /website.static.content/wwwroot/language_configuration/templates/index_severalLanguages.htm into website.static.content/wwwroot/index.htm and modify the urls and icons for the languages
For unique language:
    copy the content of /website.static.content/wwwroot/language_configuration/templates/index_defaultLanguage.htm into website.static.content/wwwroot/index.htm and modify the redirect for the language folder

Launch the powershell script "/website.static.content/script/GenerateAndPackageModule.ps1"
Your website will have been generated in the languages selected and your module is package. Your module will be available once HAS restarts.
		
## Static Website Navigator Module
Install the module "Static WebSite Navigator" in HAS
To use the module you will have to connect using credential of a Hopex or HAS account

## Debug
If you see "no static site found" when connecting to navigator --> Check that the module webiste.static.content is running and that its content is not empty 


