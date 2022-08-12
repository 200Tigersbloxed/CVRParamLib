# CVRParamLib
A Library for interacting with ChilloutVR's Animator Parameters

## Installing

This guide is split into two loaders, please follow the guide for which mod loader you use.

> ___
> ### ⚠️ **Notice!**
> 
> This mod's developer(s) and the mod itself, along with the respective mod loaders, have no affiliation with ABI!
> ___

### MelonLoader

1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader)
2. Download [CVRParamLib.dll](https://github.com/200Tigersbloxed/CVRParamLib/releases/latest/download/CVRParamLib.dll)
3. Place the dll at `[ChilloutVR]/Mods`

### BepInEx

1. Install [BepInEx](https://github.com/BepInEx/BepInEx)
    + You may need to run the game once for all required directories to appear
2. Download [CVRParamLib.dll](https://github.com/200Tigersbloxed/CVRParamLib/releases/latest/download/CVRParamLib.dll)
3. Place the dll at `[ChilloutVR]/BepInEx/plugins`

## Example Library

This example will get all Parameters, look for a Parameter called `RandomFloat`, with a ParameterType of `float` and assign it a random float each Update frame. This example depicts a BepInEx plugin, however the same can be done with a MelonLoader mod.

```cs
internal class RandomFloatParameter : BaseUnityPlugin
{
    private FullAvatarParameter randomFloatParameter;
    
    void Start()
    {
        ParameterSDK.OnLocalAvatarChanged += (avatar, info, avatarParams) =>
        {
            if (!avatarParams.Exists(x => x.Name == "RandomFloat" && x.ParameterType == typeof(float)))
            {
                randomFloatParameter = null;
                return;
            }
            foreach (FullAvatarParameter fullAvatarParameter in avatarParams)
                if (fullAvatarParameter.Name == "RandomFloat" &&
                    fullAvatarParameter.ParameterType == typeof(float))
                    randomFloatParameter = fullAvatarParameter;
        };
    }

    void Update()
    {
        if(randomFloatParameter != null)
            ParameterSDK.SetAvatarParameter(randomFloatParameter.Name, (float) new Random().NextDouble());
    }
}
```

## Checklist

- [X] MelonLoader Support
- [X] BepInEx Support
- [X] ParameterSDK
  + Interaction with CVR Animator Parameters via. a Mod/Plugin
- [X] Config
- [ ] OSC
  - [X] `/avatar/parameter` Address
  - [X] `/avatar/change` Address
  - [ ] `/settings/cvrpl` Address
    + Ability to change config via. OSC
  - [ ] `/input` Address
    - [ ] `Vertical`
    - [ ] `Horizontal`
    - [ ] `LookHorizontal`
    - [ ] `UseAxisRight`
    - [ ] `GrabAxisRight`
    - [ ] `MoveHoldFB`
    - [ ] `SpinHoldCwCcw`
    - [ ] `SpinHoldUD`
    - [ ] `SpinHoldLR`
    - [ ] `MoveForward`
    - [ ] `MoveBackward`
    - [ ] `MoveLeft`
    - [ ] `MoveRight`
    - [ ] `LookLeft`
    - [ ] `LookRight`
    - [ ] `Jump`
    - [ ] `Run`
    - [ ] `ComfortLeft`
    - [ ] `ComfortRight`
    - [ ] `DropRight`
    - [ ] `UseRight`
    - [ ] `GrabRight`
    - [ ] `GrabLeft`
    - [ ] `PanicButton`
    - [ ] `QuickMenuToggleLeft`
    - [ ] `QuickMenuToggleRight`
    - [ ] `Voice`
