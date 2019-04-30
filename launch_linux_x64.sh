ask() {
    # http://djm.me/ask
    while true; do

        if [ "${2:-}" = "Y" ]; then
            prompt="Y/n"
            default=Y
        elif [ "${2:-}" = "N" ]; then
            prompt="y/N"
            default=N
        else
            prompt="y/n"
            default=
        fi

        # Ask the question (not using "read -p" as it uses stderr not stdout)
        echo -n "$1 [$prompt] "

        # Read the answer (use /dev/tty in case stdin is redirected from somewhere else)
        read REPLY </dev/tty

        # Default?
        if [ -z "$REPLY" ]; then
            REPLY=$default
        fi

        # Check if the reply is valid
        case "$REPLY" in
            Y*|y*) return 0 ;;
            N*|n*) return 1 ;;
        esac

    done
}

RED='\033[0;31m'
NC='\033[0m'

echo "!> Updating... "
git pull > /dev/null 2>&1
if [[ $? -ne 0 ]]; then
	(>&2 echo -e "${RED}error:${NC} Failed to sync the repository (git pull cannot be executed due to conflicts)!")
	if ask "!> Force the sync of the repository (recommended)?"; then
		git fetch origin
		git reset --hard origin/master
		if ask "!> Clean all auxiliary files and directories (this will delete your bot config)?"; then
			git clean -fdx
		fi
	else
		exit 1
	fi
fi

cd TheGodfather

echo "!> Building (this may take a while)... "
dotnet build > /dev/null 2>&1
if [[ $? -ne 0 ]]; then
	(>&2 echo -e "${RED}critical:${NC} Failed to build the bot. This should not happen.")
	exit $?;
fi

echo "!> Updating database... "
dotnet ef database update > /dev/null 2>&1

echo "!> All done! Starting the bot... "
until dotnet run; do
	echo "!> TheGodfather crashed with exit code $?. Relaunching in 5 seconds..." >&2
	sleep 5
done
