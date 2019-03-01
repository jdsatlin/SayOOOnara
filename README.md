# SayOOOnara
.NET Core based web app to track out of office users in Slack and post a daily broadcast to keep everyone in the loop.

## Concept
SayOOONara is a .NET Core/Kestrel based web app, intended to function as a very lightweight, always-on web-server tracking, recording, and presenting the users who are out of office in your organization, right in the place you're most likely to try and get in contact with them, Slack.  
The core workflow consists of users typing a /OOO command in Slack, along with an optional begin date, end date, and message. These will be intelligently defaulted when left out.
SayOOONara then uses this information to post one or more daily broadcasts of all the users who are out of office that day into a channel you specify. 

## Install Instructions
SayOOONara is meant to run with a bare minimum of configuration and upkeep, including operating under only the absolutely necessary permissions.

The core points of configuration are: 
 * A /OOO (or similarly titled) command, set up under Slack's slash commands. This will need to point to a publically facing address, so the Slack API can push the command to SayOOONara's controller.  
 * A /OOOReturn (or similarly titled) command, also set up under Slack's slash commands, which allow users to mark themselves back in office early or cancel upcoming out of office periods.  
 * An OAUTH token with Channels:Write permission in order to be able to post broadcasts to the channel of your choosing added the application's options.config.  

You can use the appsettings.json file to set up SayOOONara with bindings in either HTTP or HTTPs for the publically facing addresses, or reverse proxy it behind IIS/Nginx to handle that for you.
The options.config.sample and appsettings.json.sample files included in the repository have the information you need to configure SayOOONara's broadcast options.  

## Technical Details
SayOOONara was built largely as a learning project, to gain familiarity with .NET Core and Kestrel, while also solving a real problem I encounter day-to-day. 
It runs on Kestrel, and is intended to be distributed as a standalone application. Storage is backed by a lightweight SQLite database, accessed via EntityFramework which gets automatically generated on first run to make updating as easy as possible.
The bulk of the business logic lies in the the various /OOOBotCore/Slack/ folder, in particular the most detailed logic is in the slash command handlers ([SlashOooHandler.cs](../master/OOOBotCore/Slack/SlashOooHandler.cs) & [SlashReturnHandler.cs](../master/OOOBotCore/Slack/SlashReturnHandler.cs)) with natural language date/time parsing and support for interactive buttons on Slack.  
These classes are backed by relatively simple data classes for the out of office period ([OooPeriod.cs](../master/OOOBotCore/Slack/OooPeriod.cs) and User ([User.cs](../master/OOOBotCore/Slack/User.cs))
 

