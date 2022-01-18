# BigBoxAutoPlay
BigBoxAutoPlay is a plug-in for BigBox that can automatically launch into a game when BigBox starts up  

## Installation
1.  Copy BigBoxAutoPlay.dll into the LaunchBox\Plugins folder
2.  Start BigBox for the first time - a folder and settings file will be created: LaunchBox\Plugins\BigBoxAutoPlay\settings.json
3.  Open settings.json in notepad.  See the settings section for options on what game should automatically start when BigBox loads
4.  Make changes as desired and save the settings file
5.  Exit and restart BigBox - if a game is found that matches the criteria specified in the settings file, it will automatically launch

## Settings
When BigBox loads for the first time, a settings file will be created that will allow you specify how the plugin should select a game to launch on startup.  The default settings look like this: 

```json
{
  "AutoPlayType": "Off",
  "OnlyFavorites": false,
  "Playlist": "",
  "Platform": "",
  "GameTitle": ""
}
```

## AutoPlayType
The determination of which game should automatically start is based on the "AutoPlayType" setting.  Specify one of the following values: 

### "Off"
This is the default.  The plug-in won't do anything if set to "Off".

### "RandomGame"
When the AutoPlayType is set to "RandomGame", the plug-in will select a random game from all games in your LaunchBox library. 
```json
{
  "AutoPlayType": "RandomGame",
  "OnlyFavorites": false,
  "Playlist": "",
  "Platform": "",
  "GameTitle": ""
}
```

### "RandomFavorite"
When the AutoPlayType is set to "RandomFavorite", the plug-in will select a random game from all games in your LaunchBox library that is marked as a favorite.
```json
{
  "AutoPlayType": "RandomFavorite",
  "OnlyFavorites": false,
  "Playlist": "",
  "Platform": "",
  "GameTitle": ""
}
```

### "RandomGameFromPlaylist"
When the AutoPlayType is set to "RandomGameFromPlaylist", the plug-in will select a random game from the specified playlist.  Enter the name of a playlist in the Playlist setting.  For instance, if you have a playlist called "My top 100 games", your settings file should look like: 
```json
{
  "AutoPlayType": "RandomGameFromPlaylist",
  "OnlyFavorites": false,
  "Playlist": "My top 100 games",
  "Platform": "",
  "GameTitle": ""
}
```

To only select from games marked as favorite within the specified playlist, set the "OnlyFavorites" setting to "true" as follows: 

```json
{
  "AutoPlayType": "RandomGameFromPlaylist",
  "OnlyFavorites": "true",
  "Playlist": "My top 100 games",
  "Platform": "",
  "GameTitle": ""
}
```

### "RandomGameFromPlatform"
When the AutoPlayType is set to "RandomGameFromPlatform", the plug-in will select a random game from the specified platform.  Enter the name of a platform in the Platform setting.  For instance, if you have a platform called "Nintendo Entertainment System", your settings file should look like: 
```json
{
  "AutoPlayType": "RandomGameFromPlatform",
  "OnlyFavorites": false,
  "Playlist": "",
  "Platform": "Nintendo Entertainment System",
  "GameTitle": ""
}
```

To only select from games marked as favorite within the specified playlist, set the "OnlyFavorites" setting to "true" as follows: 

```json
{
  "AutoPlayType": "RandomGameFromPlatform",
  "OnlyFavorites": "true",
  "Playlist": "",
  "Platform": "Nintendo Entertainment System",
  "GameTitle": ""
}
```
  
### "SpecificGame"
When the AutoPlayType is set to "SpecificGame", the plug-in will select the game that has the specified title in the specified platform.  Enter both the name of a platform in the Platform setting and the title of the game in the GameTitle setting.  For instance, if you want to automatically launch into "Super Mario Bros." from the "Nintendo Entertainment System" platform, your settings file should look like: 
```json
{
  "AutoPlayType": "SpecificGame",
  "OnlyFavorites": false,
  "Playlist": "",
  "Platform": "Nintendo Entertainment System",
  "GameTitle": "Super Mario Bros."
}
```
  
### "OnlyFavorites"
Enter either "true" or "false".  This setting can be used along with the AutoPlayTypes of "RandomGameFromPlatform" and "RandomGameFromPlaylist".  If "false" then the random selection will pick any game for the specified platform/playlist.  If "true" then the random selection will only pick from games in the specified platform/playlist if they are flagged as favorites.
