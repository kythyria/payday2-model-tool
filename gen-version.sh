#!/bin/bash

cd "`dirname "$0"`"

cd PD2ModelParser/Properties

cat << EOD > AssemblyInfo.cs
// WARNING: This file is generated! Do not modify it, your
// changes will be overwritten during the build process. Instead,
// modify AssemblyInfo.i.cs, from which this file is generated.
//
EOD

if [ "$1" == "Release" ]; then
	VER=`git describe --dirty=-modified`
	if [[ $VER =~ v([0-9]+\.[0-9]+\.[0-9]+) ]]; then
		VPRE=${BASH_REMATCH[1]}
	else
		echo "ERR: Describe ($VER) does not contain version in correct format!"
		rm -f AssemblyInfo.cs
		exit 1
		#VPRE="1.0.0"
	fi
	VN="$VPRE.` git rev-list --count HEAD `"
	sed "s/\"1.0.0.0\"/\"$VN\"/g;s/Debug Build/$VER/g" AssemblyInfo.i.cs >> AssemblyInfo.cs
else
	cat "AssemblyInfo.i.cs" >> "AssemblyInfo.cs"
fi

dos2unix "AssemblyInfo.cs"

