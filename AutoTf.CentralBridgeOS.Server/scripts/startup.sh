#!/bin/bash

exec > /dev/tty1 2>&1

eval "$(ssh-agent -s)"
    
ssh-add /home/CentralBridge/github

check_internet() {
  sleep 5
    echo "Checking lan connection"
    if ethtool eth0 | grep -q "Link detected: yes"; then
        echo "Internet is connected."
        return 0 
    else
        echo "No internet connection. Not pulling updates..."
        return 1 
    fi
}

check_git_changes() {
    echo "Checking for changes in the repository..."
    git fetch

    LOCAL=$(git rev-parse @)
    REMOTE=$(git rev-parse @{u})

    if [ "$LOCAL" = "$REMOTE" ]; then
        echo "No new changes detected."
        return 1
    else
        echo "New changes detected. Pulling updates..."
        return 0  
    fi
}


cd /home/CentralBridge/AutoTf.CentralBridge/AutoTf.CentralBridge


if check_internet; then
    if check_git_changes; then
        git pull
        dotnet build -m
    else
        echo "Skipping git pull as there are no changes."
    fi
fi

dotnet run 2>&1 | tee /dev/tty1
