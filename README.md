# VideoCatalog

Author: **Ivan Monteiro Cantalice**
Video catalog shows a page with all your videos the configured directory and you can get a preview of each video when hovering. The UI is browser-based and **self-hosted**. 

## Motivation

I wanted a video catalog that was lightweight and provided multiple thumbnails for videos. 
I also wanted to build a self-hosted web app for that, so the **VideoCatalog.Webserver.exe** does exactly that:
-Load the server
-Serve the web application
-Open web browser at server's localhost:PORT
-Closes server when you press ctrl+c or close the window

## How It Works

Uses CassiniDev's library to self-host an ASP.NET MVC 4 web application (the frontend of VideoCatalog). Also uses ffmpeg library to generate thumbnails for each movie.
On post build events, copies the output of the asp.net mvc project

## Prerequisites

.Net Framework 4.0 (https://www.microsoft.com/pt-br/download/details.aspx?id=17851)

## Usage

**VideoCatalog.Thumbgen.exe** - Use this to generate the thumbnails using ffmpeg
Usage:
-path="C:\MyMovies"   Main directory for thumbnail processing.
-recursive or -r        Recursive processing option (Default: false).
-force or -f            Force recreation of all thumbs (Default: false).
-verbose or -v          Outputs information messages (Default: false).

**config.xml** - put the base folder of your movie library

**VideoCatalog.WebServer.exe** - Console application that launches the web server.
Just open and a new browser window will open at the video catalog page.

## Built With

-CassiniDev for self-hosting asp.net mvc web application
-Bootstrap and jQuery
-FFMPEG - To generate thumbnails from video