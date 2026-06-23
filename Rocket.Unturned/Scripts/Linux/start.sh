#!/bin/bash
# This script starts an Unturned 3 server on Linux machines
# To start servers with this script, place it next to the Unturned executable
# Syntax: start.sh <instance name>
# Author: fr34kyn01535 (RocketLauncher path removed in RocketModFix)

export MONO_IOMAP=all

INSTANCE_NAME=$1
STEAMCMD_HOME="$PWD/../steamcmd"
UNTURNED_HOME="$PWD"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLLOW='\033[0;33m'
NC='\033[0m'

if [ -z "$INSTANCE_NAME" ]; then
	echo "Usage: start.sh <instance name>"
	exit 1
fi

STEAMCMD_API=$STEAMCMD_HOME/linux64/steamclient.so
UNTURNED_API=$UNTURNED_HOME/lib
printf "Steam: "
if [ -f $STEAMCMD_API ]; then
	if diff $STEAMCMD_API $UNTURNED_API >/dev/null ; then
		printf "${GREEN}UP TO DATE${NC}\n\n"
	else
		cp $STEAMCMD_API $UNTURNED_API
		printf "${YELLLOW}UPDATING${NC}\n\n"
	fi
else
	printf "${RED}NOT FOUND${NC}\n\n"
fi

cd $UNTURNED_HOME

EXECUTABLE=""
for candidate in Unturned_Headless.x86_64 Unturned.x86_64 Unturned_Headless.x86 Unturned.x86; do
	if [ -f "$candidate" ]; then
		EXECUTABLE="$candidate"
		break
	fi
done

if [ -z "$EXECUTABLE" ]; then
	echo "Unturned executable not found."
	exit 1
fi

ulimit -n 2048
export LD_LIBRARY_PATH=$UNTURNED_HOME/lib:$LD_LIBRARY_PATH
mkdir -p "Servers/$INSTANCE_NAME/Rocket"

while true; do
	echo "[$(date)] Unturned started."
	./"$EXECUTABLE" -logFile "Servers/$INSTANCE_NAME/Rocket/Unturned.log" -nographics -batchmode -silent-crashes "+secureserver/$INSTANCE_NAME"
	echo "[$(date)] WARNING: Unturned closed or crashed, restarting."
	sleep 3
done
