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
Description=Run startup script
After=network.target

[Service]
Type=simple
ExecStart=/bin/bash /home/display/AutoTf.CentralBridgeOS/AutoTf.CentralBridgeOS/scripts/startup.sh
User=root
Group=root

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl enable startupScript.service
sudo systemctl start startupScript.service

echo "Rebooting the system..."
sudo reboot