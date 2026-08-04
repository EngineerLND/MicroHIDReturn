using AdminToys;
using InventorySystem.Items.MicroHID.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace MicroHIDReturn;

public class EventsHandler : CustomEventsHandler
{
    private static Config Config => Plugin.Singleton.Config;
    private MicroPedestal _currentRoundPedestal = null!;
    private InteractableToy _currentRoundToy = null!;
    private AudioPlayer  _currentRoundAudioPlayer = null!;
    private bool _pickupTracking = true;
    
    
    public override void OnServerRoundStarted() {
        _currentRoundPedestal = MicroPedestal.List.First();

        _pickupTracking = _currentRoundPedestal.Base.NetworkOpenedChambers == 1;
        if(_pickupTracking)
            createInteractableToy();
    }


    public override void OnServerRoundRestarted() {
        _currentRoundPedestal = null!;
    }


    public override void OnPlayerPickedUpItem(PlayerPickedUpItemEventArgs ev) {
        if(ev.Item is not MicroHIDItem)
            return;
        
        if(_currentRoundPedestal is null)
            return;
        
        if (!_pickupTracking && _currentRoundPedestal.Base.NetworkOpenedChambers == 0) {
            _pickupTracking = true;
            createInteractableToy();
        }
    }


    public override void OnPlayerSearchingToy(PlayerSearchingToyEventArgs ev) {
        if (_currentRoundToy is null) {
            return;
        }
        
        if(ev.Interactable != _currentRoundToy)
            return;
        
        if (ev.Player.CurrentItem is not MicroHIDItem) {
            ev.IsAllowed = false;
            if (ev.Player.Items.FirstOrDefault() is MicroHIDItem) {
                ev.Player.SendHint(Config.NotInHands);
            }
            else {
                ev.Player.SendHint(Config.NoMicroHID);
            }
        }
    }

    
    public override async void OnPlayerSearchedToy(PlayerSearchedToyEventArgs ev) {
        if(_currentRoundToy is null)
            return;
        
        if(ev.Interactable != _currentRoundToy)
            return;

        if (ev.Player.CurrentItem is not MicroHIDItem microItem) {
            Logger.Warn("Player interacted with toy, but without Micro HID!");
            ev.Player.SendHint("<color=red>This is not Micro HID. Wait... how did you do that?</color>");
            return;
        }
        
        LockerChamber chamber = _currentRoundPedestal.Chambers.First();
        MicroHIDPickup microHidPickup = AddExistingItem(microItem, chamber);
        RegisterMicroAsLoot(microItem, microHidPickup, _currentRoundPedestal, chamber);

        _pickupTracking = false;

        if (Config.ChargeOnPedestal) {
            ChargeMicroHIDOnPedestal(microHidPickup);
            ev.Player.SendHint(Config.PlacedMicroHIDCharging);
        }
        else {
            ev.Player.SendHint(Config.PlacedMicroHID);
        }
        
        await Task.Delay(50);
        _currentRoundToy.Destroy();
    }


    private MicroHIDPickup AddExistingItem(MicroHIDItem microHidItem, LockerChamber microChamber) {
        Pickup pickup = microHidItem.DropItem();

        if (pickup.Base.TryGetComponent(out Rigidbody rigidbody)) {
            rigidbody.isKinematic = true;
            rigidbody.transform.ResetLocalPose();
        }

        microChamber.Base.GetSpawnpoint(ItemType.MicroHID, 0, out Vector3 worldPosition, out Quaternion worldRotation, out Transform parent);
        pickup.Position = worldPosition;
        pickup.Rotation = worldRotation;
        pickup.Transform.SetParent(parent);
        
        return pickup as MicroHIDPickup ?? throw new InvalidCastException("Failed to cast Pickup to MicroHIDPickup");
    }
    
    private void RegisterMicroAsLoot(MicroHIDItem microHidItem, MicroHIDPickup microHidPickup, MicroPedestal microPedestal, LockerChamber microChamber) {
        microHidPickup.Base.OnSelfDestroyed += () => ReleaseConnectionWithPickup(microPedestal);
        microPedestal.Base._isTrackingPickup = true;
        microPedestal.Base.NetworkOpenedChambers = 0;
        microPedestal.Base._trackedPickup = microHidPickup.Base._transform;
        if (microHidItem.Base.TryGetSubcomponent(out DrawAndInspectorModule ret))
            ret.ServerRegisterSerial(microHidPickup.Serial);
    }

    private void ReleaseConnectionWithPickup(MicroPedestal microPedestal) {
        microPedestal.Base.NetworkOpenedChambers = 1;
        microPedestal.Base._isTrackingPickup = false;
    }
    
    private async void ChargeMicroHIDOnPedestal(MicroHIDPickup pickup) {
        if (pickup.Energy <= 0.99) {
            const float maxEnergy = 1f;
            PlaySound("Start");
        
            while (pickup != null && pickup.Base != null && !_pickupTracking && _currentRoundPedestal.Base.NetworkOpenedChambers == 0) {
                await Task.Delay(1000);
                if (pickup == null || pickup.Base == null || _pickupTracking || _currentRoundPedestal.Base.NetworkOpenedChambers != 0) {
                    break;
                }

                float currentEnergy = pickup.Energy;

                if (currentEnergy >= maxEnergy) {
                    break;
                }
                
                PlaySound("Loop");
                pickup.Energy = Mathf.Min(currentEnergy + Config.ChargeStep, maxEnergy);
            }

            PlaySound("End");
        }
        PlaySound("Beeps");
    }
    
    private void createInteractableToy() {
        if (_currentRoundPedestal is null) {
            Logger.Warn("CurrentRoundPedestal Toy is null!");
            return;
        }
        Vector3 pos = new Vector3(
            _currentRoundPedestal.Transform.position.x,
            _currentRoundPedestal.Transform.position.y + 1.2f,
            _currentRoundPedestal.Transform.position.z);
        Vector3 scale = new Vector3(
            0.4f,
            0.4f,
            0.4f);
        _currentRoundToy = InteractableToy.Create(pos, _currentRoundPedestal.Transform.rotation);
        _currentRoundToy.Scale = scale;
        _currentRoundToy.InteractionDuration = 5f;
        _currentRoundToy.Shape = InvisibleInteractableToy.ColliderShape.Box;
        //debug things for _currentRoundToy
        //DrawableLines.IsDebugModeEnabled = true;
        //DrawableLines.GenerateBounds(new Bounds(currentRoundToy.Transform.position,currentRoundToy.Scale), 3f ,new Color(0.8f, 0, 0));
    }
    
    private void PlaySound(string sound) {
        if (_currentRoundPedestal is null) {
            Logger.Warn("CurrentRoundPedestal is null!");
            return;
        }
        
        if (Config.PlayChargingSound) {
            _currentRoundAudioPlayer = AudioPlayer.CreateOrGet("microhid_pedestal_audioPlayer", onIntialCreation: (p) => {
                p.transform.parent = _currentRoundPedestal.Transform;
                Speaker speaker = p.AddSpeaker("speaker_pedestal", isSpatial: true, minDistance: 1f, maxDistance: 5f);
                speaker.transform.parent = _currentRoundPedestal.Transform;
                speaker.transform.localPosition = new Vector3(0,1.2f,0);
            });
            _currentRoundAudioPlayer.DestroyWhenAllClipsPlayed = false;
            if(sound == "End")
                _currentRoundAudioPlayer.RemoveAllClips();
            _currentRoundAudioPlayer.AddClip(sound);
        }
    }
}