# AgilefantTimes

Getting the hours spent for all members in Agilefant is a time consuming process and difficult, expecially when you just want an overview. This project is to create a simple web-based dashboard that can display times for users and some simple statistics regarding their hours.

## Download
Release versions can be found [here](https://github.com/mrkno/AgilefantTimes/releases).<br>
For up to date snapshots, please clone the source and compile yourself.

## Usage
During this development phase it requires manual setup.<br>
In the same directory as the compiled .exe create a new file called <code>aftimes.conf</code>.<br>
Inside the file paste the following configuration, edited where appropriate:<br>
```
{
	"Username":"abc12",
	"Password":"12345678",
	"TeamNumber":1,
	"SprintNumber":2,
	"DisplayUsercode":false // optional, will replace Names with UserCodes.
}
```
To run the program, just double click the `AgilefantTimes.exe` executable, and, provided everything is setup correctly and you did not encounter a bug it will run.<br>
On platforms other than Windows the executable **should** be able to be run using Mono, provided the version being used supports all the features used in the program. To do this run from within a shell:<br>
```mono AgilefantTimes.exe```<br>

* Known NOT to be working on Mono 3.2.8 (what is on the lab machines).
* Known to be working on 3.12.1 which can be aquired for many linux distros [here](http://software.opensuse.org/download.html?project=home%3Atpokorra%3Amono&package=mono-opt). If you are on RedHat use the CentOS release.

<br>Alternatively provided Wine is setup correctly it could be run using:<br>
```wine AgilefantTimes.exe```

### GetJson.php and index.html
`GetJson.php` can be used to extract the output of the .exe into a php or html webpage. To use set the variables inside the variables section of the file.<br>
`index.html` is a quick and dirty example of how the data could be displayed on a webpage.<br>
A working example of these files can be seen [here](http://csse-s302g1.canterbury.ac.nz/).

## Known Working Minimum Requirements
.NET 4.5 or Mono 3.12.1 (see above).
This application is currently targeted at the .NET 4.5 runtime so you either need that installed or a compatible version of .NET/Mono.

## License and Contributions
This program is licensed under the MIT license. Pull requests and contributions welcome.
