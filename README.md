# AutoTf.CentralBridge

## Basic explanation

The central bridge is the central API to interact with the train via motors, cameras, and other motors.


It calculates paths, brake ways and more internally, but parses things like object detection onto the [AIC unit](https://github.com/AutoTf-Rail/AutoTf.Aic).


On startup it creates a bluetooth beacon, which contains it's SSID, with which you can connect yourself with the password "CentralBridgePW" (not secure on purpose.

Afterwards your device has to login at /information/login with its mac address, and the serial number as well as current code of your yubikey.

The yubikey has to be added and synced in the Central server via the [AutoTf.Manager App](https://github.com/AutoTf-Rail/AutoTf.manager).


If a device is not logged in, it will be kicked off the network.


Certain endpoints marked by the "[MacAuthorize]" attribute can only be reached if your device is logged in, or if the request is coming from a IP range 192.168.0.xxx

This is used by the AIC unit to transfer data locally without needing to log in.


It is recommended to use [AutoTf.TabletOS](https://github.com/AutoTf-Rail/AutoTf.TabletOS) with a Raspberry PI and a touch display to reach maximum ease of use.


## API documentation
The API documentation can be found at [docs.autotf.de/centralbridge](https://docs.autotf.de/centralbridge)

## Info & Contributions

Further documentation can be seen in [AutoTF-Rail/AutoTf-Documentation](https://github.com/AutoTF-Rail/AutoTf-Documentation)

To initialize this repository run the following command:

`git submodule update --init --recursive`



Would you like to contribute to this project, or noticed something wrong?

Feel free to contact us at [opensource@autotf.de](mailto:opensource@autotf.de)
