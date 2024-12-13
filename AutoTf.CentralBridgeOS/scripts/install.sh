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

if systemctl list-units --full --all | grep -Fq 'osStartupScreenScript.service'; then
    echo "Disabling and removing existing osStartupScreenScript service..."
    sudo systemctl stop osStartupScreenScript.service
    sudo systemctl disable osStartupScreenScript.service
    sudo rm -f /etc/systemd/system/osStartupScreenScript.service
fi

echo "Setting up the script to run at startup..."

cat <<EOF | sudo tee /etc/systemd/system/startupScript.service
[Unit]
Description=Run startup script
After=network.target

[Service]
Type=simple
ExecStart=/bin/bash /home/display/AutoTf.TabletOS/AutoTf.TabletOS/bin/Debug/net8.0/scripts/startup.sh
User=root
Group=root

[Install]
WantedBy=multi-user.target
EOF

cat <<EOF | sudo tee /etc/systemd/system/osStartupScreenScript.service
[Unit]
Description=Run os startup script
After=multi-user.target getty@tty1.service
Before=tty1.service

[Service]
Type=simple
ExecStart=/bin/bash /home/display/AutoTf.TabletOS/AutoTf.TabletOS/bin/Debug/net8.0/scripts/showStartupScreen.sh
Restart=no

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl enable startupScript.service
sudo systemctl start startupScript.service
sudo systemctl enable osStartupScreenScript.service
sudo systemctl start osStartupScreenScript.service

sudo systemctl enable startupScript.service
sudo systemctl start startupScript.service

echo "Rebooting the system..."
sudo reboot