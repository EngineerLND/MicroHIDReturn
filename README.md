# MicroHIDReturn (LabAPI)

**MicroHIDReturn** is a simple plugin for SCP: SL built on **LabAPI**. It allows to return the Micro HID to its pedestal - a feature that's missing in vanilla gameplay - while adding a realistic charging mechanic with atmospheric sound effects.
---

## Features

- 🔄 **Return Micro HID to Pedestal** - Place the Micro HID back on its pedestal with your interaction button.
- ⚡ **Charging Mechanics** - Recharge your Micro HID at the pedestal with fully configurable charge speed (Optional. Can be turned off in config.).
- 🔊 **Atmospheric Sounds** - Sounds during the charging process (optional).
- 🧩 **Custom Micro HID Compatible** - Works seamlessly with plugins that add custom Micro HID variants (doesn't create new items, uses the one from your hand)
- ⚙️ **Fully Configurable** - Toggle features on/off and adjust values to your liking

---

## Installation

1. **Download** the `.dll` file from the [Releases](https://github.com/EngineerLND/MicroHIDReturn/releases) page.

2. **Navigate** to the LabAPI plugins folder:
   ```
   %APPDATA%\SCP Secret Laboratory\LabAPI\plugins
   ```

3. **Place** the `.dll` file either:
   - In a **specific port folder** (e.g., `7777/`) for per-server usage, or
   - In the **global** folder to apply to all servers

4. **Launch** your server once to generate the configuration file:
   ```
   %APPDATA%\SCP Secret Laboratory\LabAPI\configs\(your_port)\MicroHIDReturn
   ```

---

## Configuration

| Setting | Description | Default |
|---------|-------------|---------|
| `NoMicroHID` | Message shown when player doesn't have a Micro HID | `<color=red>You dont have Micro HID to place.</color>` |
| `NotInHands` | Message shown when Micro HID is not equipped | `<color=yellow>Equip Micro HID.</color>` |
| `PlacedMicroHID` | Message shown when Micro HID is placed without charging | `<color=green>You placed Micro HID on pedestal.</color>` |
| `PlacedMicroHIDCharging` | Message shown when Micro HID is placed and charging begins | `<color=green>You placed Micro HID on pedestal. It start charging.</color>` |
| `ChargeOnPedestal` | Enable/disable charging while Micro HID is on the pedestal | `true` |
| `PlayChargingSound` | Enable/disable atmospheric charging sounds (requires `ChargeOnPedestal` to be enabled) | `true` |
| `ChargeStep` | Charge amount per second. `1` = 100%, `0.01` = 1%, `0.005` = 0.5% | `0.005` |

---

## How to Use

1. Hold the Micro HID in your hand
2. Look at the pedestal
3. Interact to place it back
4. If charging is enabled, the Micro HID will gradually recharge while on the pedestal

---

## Compatibility

Fully compatible with **all custom Micro HID plugins** - no conflicts, no duplicates. The plugin simply returns the item from your hand to the pedestal without creating new instances.
