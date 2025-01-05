# Hackebein's Object Tracking
Track your real objects and track them in VRChat. All SteamVR/OpenVR supported trackers are supported. [Demo](https://x.com/Hackebein/status/1817729114142343460)

Needs App. Available on:
* [Steam](https://store.steampowered.com/app/3140770) (soon<sup>1</sup>)
* [Github](https://github.com/Hackebein/Object-Tracking-App/releases)

<sup>1</sup> beta keys can be requested via support

Hackebein's VPM Listing: [vpm.hackebein.dev](https://vpm.hackebein.dev)

## Support
* [Hackebein's Research Lab](https://discord.gg/AqCwGqqQmW) at discord.gg

### Project Overview
[Task Overview](https://github.com/users/Hackebein/projects/4)

## Versions
Everything before version 1.0 is to be seen as pre-release.

### Pre-release
Pre-releases are essentially test versions that have undergone less rigorous testing and may contain bugs. These versions have limited compatibility and are typically designed to work only with the latest provided App version.

## Setup
* Add VPM Listing: [vpm.hackebein.dev](https://vpm.hackebein.dev) to VCC
* Add VPM "Hackebein's Object Tracking" to your Project (not "Hackebein's Object Tracking Setup")
* Add Empty Game Object as child to your Avatar
* Add "Hackebein's Object Tracking Base Component" to the GameObject
* Update Tracker (Once/Continuously)

### Base Component
![Base Component](Docs/base_component.png)

**Update Once**: Generates new Game Objects for Trackers which are not on your ignore list. Updates Trackers with "Update in Edit Mode" set.

**Update Continuously**: Same as Update Once. Running on every Scene redraw.

**Add Menu**: Basic Menu to toggle objects. Also contains a toggle to stabalize your position. (You can create your own. List for Menu Items can be found below.)

### Tracker Component
![Tracker Component](Docs/tracker_component.png)

**Identifier** (if no device data): Identifier for OSC

**Position Damping**: Smoothes position. Values from 0.00 (0%) to 1.00 (100%)

**Rotation Damping**: Smoothes Rotation. Values from 0.00 (0%) to 1.00 (100%)

**Hide Beyond Limits**: Hides the objects beyond the remote/yellow, as well as beyond the local/red area

**Update in Edit Mode**: Allows to update this tracker from the Base Component

### Menu Item
#### ObjectTracking/tracker/\<SERIAL NUMBER\>/enabled
Toggle - Enables Object

#### ObjectTracking/goStabilized
Toggle - stabalize movement if a lot of movement is going on

#### ObjectTracking/isStabilized [Read Only]
#### ObjectTracking/isLazyStabilized [Read Only]
Button - shows if stabalization is active

### Debug Menu Item
#### ObjectTracking/isRemotePreview
Toggle - Switches to remote view.

#### ObjectTracking/config/global
Toggle - Resends config to App.
