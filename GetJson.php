<?php
/*
GetJSON.php - gets the times JSON from AgilefantTimes
into an HTMl page via PHP.
Licensed under the MIT license.
*/

// VARIABLES TO SETUP
$monoLocation = '/opt/mono/bin';
$exeLocation = '/var/www';
$exeName = 'AgilefantTimes.exe';
// /VARIABLES

// Set working directory to the same as the exe and backup existing dir
$cwd = getcwd();
chdir($exeLocation);

// Run command and get output
$output = array();
$command = "$monoLocation/mono $exeLocation/$exeName 2>&1";
exec($command, $output);

// Print lines to webpage
foreach ($output as $line) {
        echo $line;
}

// Restore working directory
chdir($cwd);

?>