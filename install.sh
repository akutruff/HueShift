#copy current build
curl -sSL -o HueShift.zip https://github.com/akutruff/HueShift/releases/download/0.2/HueShift.zip 
unzip HueShift.zip -d /home/pi/HueShift

chown -R pi:pi /home/pi/HueShift
chmod +x /home/pi/HueShift/HueShift
chown pi:pi HueShift.zip

echo "Now run and follow instructions:"
echo "/home/pi/HueShift/HueShift"
