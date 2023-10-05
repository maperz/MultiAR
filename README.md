# Augmented Reality Multi-user Application


## Setup Instructions

Add file based dependency to Packages folder:

```
│   manifest.json
│   manifest.json.backup
│   packages-lock.json
│
├───MixedReality
│       com.microsoft.azure.spatial-anchors-sdk.android-2.12.0.tgz
│       com.microsoft.azure.spatial-anchors-sdk.core-2.12.0.tgz
│       com.microsoft.azure.spatial-anchors-sdk.ios-2.12.0.tgz
│       com.microsoft.azure.spatial-anchors-sdk.windows-2.12.0.tgz
│       com.microsoft.mixedreality.openxr-1.4.0.tgz
│       com.microsoft.mixedreality.toolkit.examples-2.7.3.tgz
│       com.microsoft.mixedreality.toolkit.extensions-2.7.3.tgz
│       com.microsoft.mixedreality.toolkit.foundation-2.7.3.tgz
│       com.microsoft.mixedreality.toolkit.standardassets-2.7.3.tgz
│       com.microsoft.mixedreality.toolkit.tools-2.7.3.tgz
│
└───Vuforia
        com.ptc.vuforia.engine-10.11.3.tgz
```

## Insert API Keys ASA

To configure ASA to work make sure that the service is properly configured and a valid subscription exists on Azure.
Add the Azure Spatial Anchor key material to `Assets/AzureSpatialAnchors.SDK/Resources/SpatialAnchorConfig.asset`.
This includes the `Spatial Anchors Account Id`, `Spatial Anchors Account Key` and `Spatial Anchors Account Domain`


## Insert App Id to PUN

To configure PUN add the AppId to the settings assets located under `Assets/MultiAR/Dependencies/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset`.
The field required is called `App Id PUN`.
