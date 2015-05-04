<?php
/*
GetJSON.php - gets the times JSON from AgilefantTimes
into an HTMl page via PHP.
Licensed under the MIT license.
*/

ini_set('display_errors',1);
ini_set('display_startup_errors',1);
error_reporting(-1);

// VARIABLES TO SETUP
$monoLocation = '/opt/mono/bin';
$exeLocation = '/var/www';
$exeName = 'AgilefantTimes.exe';

// /VARIABLES

function get_arguments($url){

	$matches = array();

	$match = preg_match("/\/sprint\/([0-9]+)\/?/", $url, $matches);

	if (count($matches) > 1) {
		return array("sprint" => $matches[1]);
	}
	else {
		return array();
	}
}

$url = "http://$_SERVER[HTTP_HOST]$_SERVER[REQUEST_URI]";
$arguments = get_arguments($url);
$sprint = isset($arguments["sprint"]) ? "--sprint ".$arguments["sprint"] : "";

// Set working directory to the same as the exe and backup existing dir
$cwd = getcwd();
chdir($exeLocation);

// Run command and get output
$output = array();
$command = "$monoLocation/mono $exeLocation/$exeName $sprint 2>&1";
exec($command, $output);

// Print lines to webpage
foreach ($output as $line) {
        echo $line;
}

// Restore working directory
chdir($cwd);

?>