# VideoTool

A quick and dirty (and limited) command line video tool for Windows. Other OS with .NET may work, but the tool has not been tested or designed with those systems in mind.

## Commands
* yt -w KsdrUdByoPc https://www.youtube.com/watch?v=mxmD16121JU -dir <optional>
** Downloads the Youtube video and stores in Documents/YoutubeVideos
* convert
** Converts all videos in the working directory with the extensions .mkv, .flv, .avi, .mov into mp4 and renames the original file with a backup prefix.
   Depends on [Handbrake](https://handbrake.fr/) being installed and handbrakecli.exe being in PATH.