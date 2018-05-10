# HueShift for the Raspberry Pi!

Make your Phillips Hue lights automatically cool and energetic during the day and at sunrise and sunset just like Flux and NightShift.  

Install this on a Raspberry Pi, and your house rises and sets with the sun.  When a light gets turned on, it will automatically change to the right color within 10 seconds.  (Built on .NET Core 2 so this will run on Windows, and Linux as well!)

#### Science 

Theory: In order for your body to properly regulate your sleep cycle, you need to be exposed to blue light during the day and only red light at night.  This project is originally inspired from the awesome Flux that I highly recommend installing on your machine right now https://justgetflux.com/.  (Flux can control Hue too, but it isn't quite what I wanted/need.) 

HueShift does the same thing but with your Phillips Hue Light bulbs.  The program automatically discovers your location on earth by using a geolocation service against your IP address.  Then it continually calculates sunrise and sunset for your latitude and longitude.  During the daytime, your bulbs are forced to be as cool as possible.  After sunset, they are as red as possible. There's lots of configuration you can do through the command line or the generated configuration file.

## Instructions for Raspberry Pi:

#### Install .NET Core 2 for Raspberry Pi 

Get yourself a Raspberry Pi. Do the SD card thing. (Pi Zero doesn't not support arm7, so no luck there.)

Taken from: [blogs.msdn](https://blogs.msdn.microsoft.com/david/2017/07/20/setting_up_raspian_and_dotnet_core_2_0_on_a_raspberry_pi/)
```
sudo apt-get install curl libunwind8 gettext
curl -sSL -o dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Runtime/release/2.0.0/dotnet-runtime-latest-linux-arm.tar.gz 
sudo mkdir -p /opt/dotnet && sudo tar zxf dotnet.tar.gz -C /opt/dotnet
sudo ln -s /opt/dotnet/dotnet /usr/local/bin
```
Test the installation by typing. 
```dotnet --help```

It will say something wierd about installing the SDK.  Ignore that noise.  You're good.

#### Install latest build and run it.

```
curl -sSL -o HueShift.zip https://github.com/akutruff/HueShift/releases/download/0.1/HueShift.zip 
unzip HueShift.zip -d /home/pi/HueShift
cd /home/pi/HueShift
chmod +x HueShift
./HueShift
```
#### Hit the button on the Hue bridge!  

The code at the moment tries three times to connect to the bridge before the program times out.  It may spit out an exception saying it can't find the bridge.  That's okay.  Just hit the button on the front of the bridge.  It will try three times and then quit.  

If all seems okay, test it.  Change the color of your lights in the app or via Alexa.  Wait 10 seconds.  Your lights should automagically shift to blu-ish during the day, and red-ish at night.  Try it a few times, the code is checking every 10 seconds so it may override your attempt at changing the colors. (That's the point afer all.)

Once you've verified it's working, hit CTRL+C to stop the program running, and then install it as a service:

```
sudo cp hueshift.service /etc/systemd/system/hueshift.service
sudo systemctl start hueshift.service
sudo systemctl enable hueshift.service
```
Now put your Pi somewhere, and leave it onl, augh heartily, and you now have an automatic sunrise and sunset machine!  

### Donate:
To help pay for further development and allow those to benefit from turnkey geolocation and proxy service when free limits are reached, Bitcoin donations are accepted here:

bitcoin:34TxsK9Wfd8GcjMTL3uzVkxF1WoKC9qXoW

![Donate!](https://github.com/akutruff/HueShift/blob/master/img/donate.png)


### Customization:

`/home/pi/HueShift/hueShift-conf.json`

After you run HueShift the first time, a conf file will appear in the Hue directory.  It's pretty self explanatory if you crack it open you can edit the defaults and discovered values.  Make sure the service has been stopped before editing the file.  (It's quick but very stable code ya'll.)

If you have an advanced setup with your Hue running on a different subnet or if UPnP / HTTP discovery aren't working, you can crack the conf file and manually enter your hostname for the bridge.  May need to specify port 80 as well if it's not working with just the IP.
