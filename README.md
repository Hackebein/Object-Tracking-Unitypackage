# Hackebein's Object Tracking
Track your real objects and track them in VRChat. All SteamVR/OpenVR tracker are supported.

Needs SteamVR component. Available on:
* [Steam](https://store.steampowered.com/app/3140770) (soon)
* [Github](https://github.com/Hackebein/Object-Tracking-App/releases)

Hackebein's VPM Listing: [vpm.hackebein.dev](https://vpm.hackebein.dev)

## Support:
* [Hackebein's Research Lab](https://discord.gg/AqCwGqqQmW) at discord.gg

### Known Issues
* WD ON not supported

## Unity Setup Script
![Unity Setup Script](Docs/setup_script.png)

### Pre-Setup
* (Optional) Start SteamVR and connect trackers you like to use
* Add setup component by clicking<br>
  top-navigation **>** Tools **>** Hackebein **>** Object Tracking Setup

### Quick Setup (simple)
* Avatar: select your avatar<br>
  <sup>_needs to contain Avatar Descriptor component_</sup>
* Real height (in m)
* If SteamVR is Running:
  * click reload button
  * click tracker you like to add
* If SteamVR is not Running:
  * click "Add Tracker"
  * Serial Number: Can be obtained in SteamVR System Report
    <sup>_not S/N, which can be printed on your tracker_</sup>
  * Tracker Type: Select how the tracker looks in editor
* Prefab: Shows some more information about the prefab after selecting
* Setup your Tracking costs
* Press Create: Tracker gets generated at 0, 0 if "Apply last position" is unchecked
* Align your object inside the 4th object with the same name at 1:1 IRL scale

Video: https://youtu.be/oxvaUqVMznQ

### Menu Item
#### ObjectTracking/goStabilized
Toggle - stabalize movement if a lot of movement is going on

#### ObjectTracking/isStabilized [Read Only]
Button - shows if stabalization is active

### Debug Menu Item
#### ObjectTracking/isRemotePreview
Toggle - Switches to remote view. (update rate change not supported yet)

#### ObjectTracking/config/global
Toggle - Resends config to App.
