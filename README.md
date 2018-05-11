# HueShift for the Raspberry Pi!

Make your Phillips Hue lights cool during the day and warm after sunset just like Flux and Apple's Night Shift.  

Install on a Raspberry Pi, and your house rises and sets with the sun.  When a light gets turned on, it will automatically change to the right color within 10 seconds.  (Built on .NET Core 2 so this will run on Windows and Linux as well!)

Log in as the pi user:

Install .NET Core
```
curl -sSL https://raw.githubusercontent.com/akutruff/HueShift/master/install-dotnet.sh | sudo bash
```

Install HueShift (no sudo this time!)
```
curl -sSL https://raw.githubusercontent.com/akutruff/HueShift/master/install.sh | bash
```

Run it manually to authorize
```
/home/pi/HueShift/HueShift
```

Setup so it's always running even after logout
```
curl -sSL https://raw.githubusercontent.com/akutruff/HueShift/master/install-service.sh | sudo bash
```

#### Science 

In order for your body to properly regulate your sleep cycle, you need to be exposed to blue light during the day and only red light at night.  This project is originally inspired from the awesome program, Flux, that I highly recommend installing on your machine right now https://justgetflux.com/.  (Flux can control Hue too, but it doesn't get the job done.) 

HueShift automatically geolocates against your IP address and continually calculates sunrise and sunset for your latitude and longitude.  During the daytime, your bulbs are forced to be as cool as possible.  After sunset, they are as red as possible. There's lots of configuration you can do through the command line or the generated configuration file.

## Instructions for Raspberry Pi:

Get yourself a Raspberry Pi. Do the SD card thing. (Pi Zero doesn't support arm7, so no luck there. Sorry, folks.)

#### Hit the button on the Hue bridge!  

The code at the moment tries to connect to the bridge before the program times out.  It may spit out an exception saying it can't find the bridge.  That's okay.  Just hit the button on the front of the bridge.  It will try three times and then quit.  

If all seems okay, test it.  Change the color of your lights in the Hue app or via Alexa.  Wait 10 seconds.  Your lights should automagically shift to blu-ish during the day, and red-ish at night.  Try it a few times, the code is checking every 10 seconds so it may override your attempt at changing the colors. (That's the point afer all.)

Now put your Pi somewhere, and leave it on, laugh heartily, and you now have an automatic sunrise and sunset machine!  

### Donate:
To help pay for further development and allow those to benefit from turnkey geolocation and proxy service when free limits are reached, Bitcoin donations are accepted here:

bitcoin:34TxsK9Wfd8GcjMTL3uzVkxF1WoKC9qXoW

![Donate!](https://github.com/akutruff/HueShift/blob/master/img/donate.png)


### Customization:

`/home/pi/HueShift/hueShift-conf.json`

After you run HueShift the first time, a conf file will appear in the Hue directory.  It's pretty self explanatory if you crack it open you can edit the defaults and discovered values.  Make sure the service has been stopped before editing the file.  (It's quick and dirty code but stable.)

If you have an advanced setup with your Hue running on a different subnet or if UPnP / HTTP discovery aren't working, you can crack the conf file and manually enter your hostname for the bridge.  May need to specify port 80 as well if it's not working with just the IP.
