#!/bin/bash

echo "Configuring root auto-login on tty1..."
autologin_file="/etc/systemd/system/getty@tty1.service.d/autologin.conf"

sudo mkdir -p /etc/systemd/system/getty@tty1.service.d

echo "[Service]
ExecStart=
ExecStart=-/sbin/agetty --autologin display --noclear %I \$TERM" | sudo tee $autologin_file > /dev/null

sudo systemctl daemon-reload

if systemctl list-units --full --all | grep -Fq 'startupScript.service'; then
    echo "Disabling and removing existing startupScript service..."
    sudo systemctl stop startupScript.service
    sudo systemctl disable startupScript.service
    sudo rm -f /etc/systemd/system/startupScript.service
fi

echo "Setting up the script to run at startup..."

cat <<EOF | sudo tee /etc/systemd/system/startupScript.service
[Unit]
Description=Auto Start Bridge
After=network.target

[Service]
ExecStart=/usr/local/bin/dotnet run --no-build -m -c RELEASE
WorkingDirectory=/home/CentralBridge/AutoTf.CentralBridgeOS/AutoTf.CentralBridgeOS.Server
Restart=on-failure
RestartSec=5
StandardOutput=journal
StandardError=journal
Environment=DOTNET_CLI_TELEMETRY_OPTOUT=1
Environment="HOME=/home/CentralBridge"
Environment="DOTNET_CLI_HOME=/home/CentralBridge"
Environment="APP_NON_INTERACTIVE=true"

[Install]
WantedBy=multi-user.target
EOF


sudo systemctl enable startupScript.service

# Add disable_splash=1 to /boot/firmware/config.txt if it doesn't exist
config_file="/boot/firmware/config.txt"
if ! grep -q "^disable_splash=1" $config_file; then
    echo "Adding disable_splash=1 to $config_file"
    echo "disable_splash=1" | sudo tee -a $config_file > /dev/null
else
    echo "disable_splash=1 is already present in $config_file"
fi

echo "Rebooting the system..."
sudo reboot