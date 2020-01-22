# magnet-to-webui
Tool to automatically add magnet links to your qBittorrent client on a server via their WebUI API.

# Installation
Clone the repository, change the config variables and build it with Visual Studio. Run it once as administrator to register the url handler.

# Usage
Starting the tool without a magnet link will result it to start elevated if it isn't elevated yet and registers a url handler with it's current path. If you happen to move the tool, you'll have to run it again.
