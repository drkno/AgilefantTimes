#!/bin/bash

# Install Mono
cd /etc/yum.repos.d/
wget http://download.opensuse.org/repositories/home:/tpokorra:/mono/openSUSE_13.1/home:tpokorra:mono.repo
yum -y install mono-opt || true

# Install Files
mkdir /var/aftimes
cd /var/aftimes
url=$(curl -s https://api.github.com/repos/mrkno/AgilefantTimes/releases | grep browser_download_url | head -n 1 | cut -d '"' -f 4)
wget $url -O aftimes.zip
unzip aftimes.zip
rm aftimes.zip

# Setup Configuration
read -p "Enter your Agilefant username: " username </dev/tty
read -p "Enter your Agilefant password: " password </dev/tty
read -p "Enter your team number: " teamNumber </dev/tty
read -p "Enter your sprint number (or -1 for automatic mode): " sprintNumber </dev/tty
read -p "Do you want to use usercodes instead of names? [y/n] " userCodeString </dev/tty
case "$userCodeString" in
  y|Y) userCode="true"
    ;;
  n|N) userCode="false"
    ;;
  *) echo "Defaulting to no as '$userCodeString' is not a valid answer."
     userCode="false"
    ;;
esac
echo "{\"Username\":\"$username\",\"Password\":\"$password\",\"TeamNumber\":$teamNumber,\"SprintNumber\":$sprintNumber,\"DisplayUsercode\":$userCode}" > /var/aftimes/aftimes.conf
chmod 0777 /var/aftimes/aftimes.conf

# Start
service httpd stop
nohup mono AgilefantTimes.exe & disown
