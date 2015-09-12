# AgilefantTimes

Getting the hours spent for all members in Agilefant is a time consuming process and difficult, expecially when you just want an overview. This project is to create a simple web-based dashboard that can display times for users and some simple statistics regarding their hours.
[A demo can be seen here](http://times.sws.nz/).

## Download
Release versions can be found [here](https://github.com/mrkno/AgilefantTimes/releases).<br>
For up to date snapshots, please clone the source and compile yourself.<br>
If you are installing this on the University of Canterbury Jenkins servers, the following commands will automate the process:<br>
```
wget https://raw.githubusercontent.com/mrkno/AgilefantTimes/master/RedHatInstall.sh
sed -i 's/\r//' RedHatInstall.sh
chmod +x RedHatInstall.sh
./RedHatInstall.sh
```

## Usage
During this development phase it requires manual setup.<br>
In the same directory as the compiled .exe create a new file called <code>aftimes.conf</code>.<br>
Inside the file paste the following configuration, edited where appropriate:<br>
```
{
	"Username":"abc12",
	"Password":"12345678",
	"TeamNumber":1,
	"SprintNumber":2,       // set to -1 to attempt to automatically determine (defaults to 1 if not found).
	"DisplayUsercode":false // optional, will replace Names with UserCodes.
}
```
To run the program, just double click the `AgilefantTimes.exe` executable, and, provided everything is setup correctly and you did not encounter a bug it will run.<br>
The program supports command line arguments, to view these start the application with the `-h` or `--help` option. Command line arguments override values in the configuration file.<br>
On platforms other than Windows the executable **should** be able to be run using Mono, provided the version being used supports all the features used in the program. To do this run from within a shell:<br>
```mono AgilefantTimes.exe```<br>

* Known NOT to be working on Mono 3.2.8 (what is on the lab machines).
* Known to be working on 3.12.1 which can be aquired for many linux distros [here](http://software.opensuse.org/download.html?project=home%3Atpokorra%3Amono&package=mono-opt). If you are on RedHat use the CentOS release.

<br>Alternatively provided Wine is setup correctly it could be run using:<br>
```wine AgilefantTimes.exe```

## Known Working Minimum Requirements
.NET 4.5 or Mono 3.12.1 (see above).
This application is currently targeted at the .NET 4.5 runtime so you either need that installed or a compatible version of .NET/Mono.

## License and Contributions
This program is licensed under the MIT license. Pull requests and contributions welcome.

<b>Note to contributors and source code criticizers:</b>

This program is essentially 4 layers of hacks and bad code layered to create an imperfect solution to a problem.
These include:

1. Mono workarounds. There are so many of these because of defficiencies in the Mono framework that it is unbeleiveable that anyone uses Mono for anything.
2. Agilefant scraping. Agilefant does not provide a nice API to free users so in some places the only way to extract data is by scraping webpages.
3. Login workarounds. Supporting login in a webserver that has to already work around 1 and 2 means that login itself has many workarounds.
4. Polymer workarounds. Polymer and its associated libraries are sometimes 'not quite finished'.
