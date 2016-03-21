# Unity Game Jolt API
Version: 1.2.8

# Links
Home Page: http://gamejolt.com/games/other/unity-game-jolt-api/15887/
Forum Post: http://gamejolt.com/community/forums/topics/unity-api/1803/
API Tutorial: http://gamejolt.com/games/unity-game-jolt-api/news/getting-started-with-the-unity-game-jolt-api-1-2/8347/
Helper Tutorial: http://gamejolt.com/games/unity-game-jolt-api/news/getting-started-with-the-unity-game-jolt-api-helper/8709/
Demo/Documentation: http://gamejolt.com/games/other/unity-gamejolt-api-demo/14443/
Bug Tracking: https://bitbucket.org/loicteixeira/unity-game-jolt-api/issues

# Uniscript (javascript) users
In order to use the API, move the 'API', 'Helper' and 'Libraries' folders into a 'Plugins' folder.
If that folder doesn't exist, create it.

This is the directories hierarchy before:
PROJECT_FOLDER
|_ Assets
   |_ GameJoltAPI
      |_ API
      |_ Editor
      |_ Helper
      |_ Libraries
      |_ Resources
      
This is the directories hierarchy after the change:
PROJECT_FOLDER
|_ Assets
   |_ GameJoltAPI
      |_ Editor
      |_ Resources
   |_ Plugins
   	  |_ GameJoltAPI
   	     |_ API
   	     |_ Helper
   	     |_ Libraries