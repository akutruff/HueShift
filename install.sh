#install .NET Core 2
apt-get install -y curl libunwind8 gettext
curl -sSL -o dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Runtime/release/2.0.0/dotnet-runtime-latest-linux-arm.tar.gz 
mkdir -p /opt/dotnet && sudo tar zxf dotnet.tar.gz -C /opt/dotnet
ln -s /opt/dotnet/dotnet /usr/local/bin

#copy current build
curl -sSL -o HueShift.zip https://github.com/akutruff/HueShift/releases/download/0.1/HueShift.zip 
unzip HueShift.zip -d /home/pi/HueShift

chown -R pi:pi /home/pi/HueShift
chown pi:pi HueShift.zip

echo "Installed! Starting service..."
echo "Hit the button on your hue bridge."
