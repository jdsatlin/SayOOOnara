# SayOOOnara
C#/.Net Core based Slack integrated app allowing users to mark themselves out of office which uses that information to post a daily broadcast of out of office users.  

## Concept
SayOOONara is a .Net Core / Kestrel based webapp, intended to function as a very lightweight, always-on webserver tracking, recording, and presenting the users who are out of office in your organization, right in the place you're most likely to try and get in contact with them, Slack.  
The core workflow consists of users typing a /OOO command in slack, along with an optional begin date, end date, and message. These will be intelligently defaulted when left out (begin date is tomorrow when left out, end date is the day after your begin date).  
SayOOONara then uses this information to post one or more daily broadcasts of users who are out of office into the the channel you specify. Meaning if a user is going to be out of office for two weeks, you'll get unobtrusive daily reminders showing when they'll be back and any other information they wish to share without them having to take any action. 

## Install Instructions
You can install SayOOOnara by adding an app integration to your workspace at api.slack.com, and entering the application token you generate in SayOOONara's app options.config file.  
SayOOONara is meant to run with a bare minimum of configuration and upkeep, including operating under only the absolutely necessary permissions.
The core points of configuration are: 
 * A /OOO (or similarly titled) command, set up under Slack's slash commands. This will need to point to a publically facing address, so the Slack API can push the command to SayOOONara's controller.  
 * A /OOOReturn (or similarly titled) command, also set up under Slack's slash commands, which allow users to mark themselves back in office early or cancel upcoming out of office periods.  
 * An OAUTH token with Channels:Write permission in order to be able to post broadcasts to the channel of your choosing.
 
You can use the appsettings.json file to set up SayOOONara with bindings in either HTTP or HTTPs for the publically facing addresses, or reverse proxy it behind IIS/Nginx to handle that for you.
The options.config.sample and appsettings.json.sample files included in the repository have the information you need to configure SayOOONara's options.
 
## Technical Details
SayOOONara was built largely as a learning project, to gain familiarity with .Net Core and Kestrel, though one to solve a real problem I encounter day-to-day. 
It runs on Kestrel, and is intended to be distributed as a standalone application. Storage is backed by a lightweight SQLite database, accessed via EntityFramework which gets automatically generated on first run to make updating as easy as possible.
The bulk of the business logic lies in the the various /OOOBotCore/Slack/ folder, in particular the most detailed logic is in the slash command handlers (
https://github.com/jdsatlin/SayOOOnara/blob/master/OOOBotCore/Slack/SlashOooHandler.cs & https://github.com/jdsatlin/SayOOOnara/blob/master/OOOBotCore/Slack/SlashReturnHandler.cs) with natural language date/time parsing and support for interactive buttons on Slack.  
These classes are backed by relatively simple data classes for the out of office period (https://github.com/jdsatlin/SayOOOnara/blob/master/OOOBotCore/Slack/OooPeriod.cs) and User (https://github.com/jdsatlin/SayOOOnara/blob/master/OOOBotCore/Slack/User.cs)
 

