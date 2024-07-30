# Hackebein's Object Tracking
Track your real objects and track them in VRChat. All SteamVR/OpenVR tracker are supported.

Hackebein's VCC Listing: [vcc.hackebein.dev](https://vcc.hackebein.dev)

## Unity Setup Script
![Unity Setup Script](Docs/setup_script.png)

### Pre-Setup
* (Optional) Start SteamVR and connect trackers you like to use
* Add setup component by clicking<br>
  top-navigation **>** Tools **>** Hackebein **>** Object Tracking Setup

### Quick Setup (simple)
* Avatar: select your avatar (needs to contain Avatar Descriptor)
* Real eye height (in m): Your Eye height when looking straight
* If Steam VR is Running:
  * click reload button
  * click tracker you like to add
* If Steam VR is not Running:
  * click "Add Tracker"
  * Serial Number: Can be obtained in SteamVR System Report
  * Tracker Type: Select how the tracker looks
* Prefab: Shows some more information about the prefab after selecting
* Setup your Tracking costs
* Press Create: Tracker gets generated at 0, 0
* Align your object inside the 4th object with the same name at 1:1 IRL scale