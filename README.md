# HueShift for the Raspberry Pi!

Make your Phillips Hue lights shift color temperature at sunrise and sunset just like Flux and NightShift.  

Automatically makes your lights cool and energetic during the day and warm and relaxing at sunset.  Works even when the lights are powered off and on again.

Install this on a Raspberry Pi, and your house rises and sets with the sun.  When a light gets turned on, it will automatically change to the right color within 10 seconds.  (Built on .NET Core 2 so this will run on Windows, and Linux as well!)

In order for your body to properly regulate your sleep cycle, you need to be exposed to blue light during the day and only red light at night.  This project is originally inspired from the awesome Flux project that I highly recommend installing on your machine right now https://justgetflux.com/.  (Flux can control Hue too, but it isn't quite there yet.) 

HueShift does the same thing but with your Phillips Hue Light bulbs.  The program automatically discovers your location on earth by using a geolocation service against your IP address.  Then it continually calculates sunrise and sunset.  During the daytime, your bulbs are forced to be as cool as possible.  After sunset, they are as red as possible. There's lots of configuration you can do through the command line or the generated configuration file.

## Instructions for Raspberry Pi:

### Install .NET Core 2 for Raspberry Pi 

Taken from: [blogs.msdn](https://blogs.msdn.microsoft.com/david/2017/07/20/setting_up_raspian_and_dotnet_core_2_0_on_a_raspberry_pi/)
```
sudo apt-get install curl libunwind8 gettext
curl -sSL -o dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Runtime/release/2.0.0/dotnet-runtime-latest-linux-arm.tar.gz 
sudo mkdir -p /opt/dotnet && sudo tar zxf dotnet.tar.gz -C /opt/dotnet
sudo ln -s /opt/dotnet/dotnet /usr/local/bin
```
Test the installation by typing 
dotnet --help.

#### Install latest build and run it.

```
curl -sSL -o HueShift.zip https://github.com/akutruff/HueShift/releases/download/0.1/HueShift.zip 
unzip HueShift.zip -d /home/pi/HueShift
sudo cp hueshift.service /etc/systemd/system/hueshift.service
sudo systemctl start hueshift.service
sudo systemctl enable hueshift.service
```
#### Hit the button on the hue bridge!  

The code at the moment tries three times to connect to the bridge before the program times out.  The service will continually retry.

### Donate:
To help pay for further development and allow those to benefit from turnkey geolocation and proxy service when free limits are reached, Bitcoin donations are accepted here:

bitcoin:34TxsK9Wfd8GcjMTL3uzVkxF1WoKC9qXoW

![Donate!](https://github.com/akutruff/HueShift/blob/master/img/donate.png)


### Customization:

`/home/pi/HueShift/hueShift-conf.json`

After you run HueShift the first time, a conf file will appear in the Hue directory.  It's pretty self explanatory if you crack it open you can edit the defaults and discovered values.  Make sure the service has been stopped before editing the file.  (It's quick but very stable code ya'll.)

If you have an advanced setup with your Hue running on a different subnet or if UPnP / HTTP discovery aren't working, you can crack the conf file and manually enter your hostname for the bridge.  May need to specify port 80 as well if it's not working with just the IP.
