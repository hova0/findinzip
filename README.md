# findinzip
Simple utility to find text inside files inside zip files.   Saves you from having to unzip a file and then searching through the files for text.  It's probably not the fastest, BUT if you have a large zip file, it will save you tons of space.  

Syntax: 
findinzip <zip archive(s)> <file(s) to search>  -text <string to find inside files>

Requires 7z to be installed and currently only tested on Windows.  The 7z requirement is for zip files over 4GB.  
