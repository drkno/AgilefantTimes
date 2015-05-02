#!/bin/bash

# Install Mono
cd /etc/yum.repos.d/
wget http://download.opensuse.org/repositories/home:tpokorra:mono/CentOS_CentOS-6/home:tpokorra:mono.repo
yum -y install mono-opt || true

# Install Files
cd /var/www/html
wget https://raw.githubusercontent.com/mrkno/AgilefantTimes/master/Web/index.html
wget https://raw.githubusercontent.com/mrkno/AgilefantTimes/master/Web/GetJson.php
wget https://raw.githubusercontent.com/mrkno/AgilefantTimes/master/Web/Chart.min.js
cd ..
wget https://github.com/mrkno/AgilefantTimes/releases/download/v0.4/AgilefantTimes.exe

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
echo "{\"Username\":\"$username\",\"Password\":\"$password\",\"TeamNumber\":$teamNumber,\"SprintNumber\":$sprintNumber,\"DisplayUsercode\":$userCode}" > /var/www/aftimes.conf
chmod 0777 /var/www/aftimes.conf
