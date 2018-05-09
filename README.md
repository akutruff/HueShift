# HueShift

Make your Phillips Hue lights shift color temperature at sunrise and sunset just like Flux and NightShift.  

Install this on a Raspberry Pi, Windows, or Linux machine and your house rises and sets with the sun.  When a light gets turned on, it will automatically change to the right color within 10 seconds.  

In order for your body to properly regulate your sleep cycle, you need to be exposed to blue light during the day and only red light at night.  This project is originally inspired from the awesome Flux project that I highly recommend installing right now https://justgetflux.com/.  (Flux can control Hue too, but it dims your lights too much and can't be customized.)  

HueShift does the same thing but with your Phillips Hue Light bulbs.  The program automatically discovers your location on earth by using a geolocation service against your IP address.  Then it continually calculates sunrise and sunset.  During the daytime, your bulbs are forced to be as cool as possible.  After sunset, they are as red as possible. There's lots of configuration you can do through the command line or the generated configuration file.

## Instructions for Raspberry Pi:

### Install .NET Core 2 for Raspberry Pi 

Taken from: https://blogs.msdn.microsoft.com/david/2017/07/20/setting_up_raspian_and_dotnet_core_2_0_on_a_raspberry_pi/
sudo apt-get install curl libunwind8 gettext
curl -sSL -o dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Runtime/release/2.0.0/dotnet-runtime-latest-linux-arm.tar.gz 
sudo mkdir -p /opt/dotnet && sudo tar zxf dotnet.tar.gz -C /opt/dotnet
sudo ln -s /opt/dotnet/dotnet /usr/local/bin

Test the installation by typing 
dotnet --help.

Install build:


### Donate:
To help pay for further development, and turnkey geolocation and proxy service when free limits are reached, Bitcoin donations are accepted here:

bitcoin:34TxsK9Wfd8GcjMTL3uzVkxF1WoKC9qXoW

[[https://github.com/akutruff/HueShift/blob/master/img/donate.png|alt=donate]]
