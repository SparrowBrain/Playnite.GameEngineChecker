# Playnite.GameEngineChecker
![DownloadCountTotal](https://img.shields.io/github/downloads/sparrowbrain/playnite.gameenginechecker/total?label=total%20downloads&style=for-the-badge)
![LatestVersion](https://img.shields.io/github/v/release/SparrowBrain/Playnite.GameEngineChecker?label=Latest%20version&style=for-the-badge)
![DownloadCountLatest](https://img.shields.io/github/downloads/SparrowBrain/Playnite.GameEngineChecker/latest/total?style=for-the-badge)

This extension will add a tag with the name of the engine used by the game. It's compatible with PC games that are available on GOG, Steam or have a Wikipedia page. The information is obtained from PCGamingWiki.

## Credits
This extension is originally by darklinkpower.

As it is one of my favourite extensions to use and he had no time to update it after major API changes, I had a crack at re-implementing it in C#. darklinkpower was kind enough to let me take over the maintenance of this extension.

## How does it work?

The extension skips games that:
* Don't have PC as their platform
* Have any `[Engine]` tags

It then queries PCGamingWiki for game pages based on:
* Steam Id, if game is from Steam library
* GOG Id, if game is from GOG library
* Otherwise we look at the links:
    * Using Steam Id, if links contain Steam store page
    * Using Wikipedia page, if one exists within links
* Otherwise the game is skipped

If multiple PCGamingWiki pages are found, the game is ignored, as my testing showed, that it could lead to false engines being assigned.

## Translation
You can help with translation by visiting [the project on Crowdin](https://crowdin.com/project/playnite-game-engine-checker).