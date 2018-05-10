

cp /home/pi/HueShift/hueShift.service /etc/systemd/system/hueShift.service
systemctl start hueShift.service
systemctl enable hueShift.service