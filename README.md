# AgilefantTimes

Getting the hours spent for all members in Agilefant is a time consuming process and difficult, expecially when you just want an overview. This project is to create a simple web-based dashboard that can display times for users and some simple statistics regarding their hours.

## Usage
During this development phase it requires manual setup.<br>
In the same directory as the compiled .exe create a new file called <code>aftimes.conf</code>.<br>
Inside the file paste the following configuration, edited where appropriate:<br>
```
{
	"Username":"abc12",
	"Password":"12345678",
	"TeamNumber":1,
	"SprintNumber":2
}
```
To run the program, just double click the `AgilefantTimes.exe` executable, and, provided everything is setup correctly and you did not encounter a bug it will run.<br>
On platforms other than Windows the executable **should** be able to be run using Mono, provided the version being used supports all the features used in the program. To do this run from within a shell:<br>
```mono AgilefantTimes.exe```
<br>Alternatively provided Wine is setup correctly it could be run using:<br>
```wine AgilefantTimes.exe```

This application is currently targeted at the .NET 4.5 runtime so you either need that installed or a compatible version of .NET/Mono.

This program is licensed under the MIT license. Pull requests and contributions welcome.
