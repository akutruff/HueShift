#copy current build
cd /home/pi
curl -sSL -o HueShift.zip https://github.com/akutruff/HueShift/releases/download/0.2/hue-shift.tar.gz 

tar -zxvf hue-shift.tar.gz 

chown -R pi:pi /home/pi/HueShift
chmod +x /home/pi/HueShift/HueShift
chown pi:pi HueShift.zip

echo "Now run this:"
echo "/home/pi/HueShift/HueShift"
