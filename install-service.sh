curl -sSL -o /home/pi/hueShift.service https://raw.githubusercontent.com/akutruff/HueShift/master/HueShift/hueShift.service

cp /home/pi/HueShift/hueShift.service /etc/systemd/system/hueShift.service
systemctl start hueShift.service
systemctl enable hueShift.service