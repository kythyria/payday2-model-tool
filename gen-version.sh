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
	VN="1.0.0.` git rev-list --count HEAD `"
	VER=`git describe --dirty=-modified`
	sed "s/\"1.0.0.0\"/\"$VN\"/g;s/Debug Build/$VER/g" AssemblyInfo.i.cs >> AssemblyInfo.cs
else
	cat "AssemblyInfo.i.cs" >> "AssemblyInfo.cs"
fi

dos2unix "AssemblyInfo.cs"

