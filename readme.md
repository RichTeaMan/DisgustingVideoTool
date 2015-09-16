﻿# VideoTool

A quick and dirty (and limited) command line video tool for Windows. Other OS with .NET may work, but the tool has not been tested or designed with those systems in mind.

## Commands
* yt -w KsdrUdByoPc https://www.youtube.com/watch?v=mxmD16121JU -dir <optional>
  * Downloads the Youtube video and stores in Documents/YoutubeVideos
* convert
  * Converts all videos in the working directory with the extensions .mkv, .flv, .avi, .mov into h.264 mp4 and renames the original file with a backup prefix.
   Depends on [Handbrake](https://handbrake.fr/) being installed and handbrakecli.exe being in PATH.
   * Use -mp4 flag to convert existing mp4 files. This can be used for mp4 files not using h.264.
* list-backups
  * List backup video files in the current directory made during converts.
* delete-backups
  * Permanently delete backup video files in the current directory made during converts.