# react-native-unity-view

Integrate unity3d within a React Native app. Add a react native component to show unity. Works on Android, and on iOS  **UNTESTED**.

# Notice

This is a fork of [asmadsen/react-native-unity-view](https://github.com/asmadsen/react-native-unity-view) to make it work with node 16 LTS, React Native >= 0.63 and Unity 2020.3 LTS

**This project may or may not be updated depending on the further use of it, feel free to fork it**

# Requirements

 - Unity 2020.3+
 - Nodejs 16.14+
 - React Native 0.63


# Prerequisites

Before anything, a React-Native app is needed, but **beware**, do not use `Expo` nor `create-react-native-app` which uses `Expo` or you'll have to eject it

```bash
# Install nvm
## for macos or linux
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.1/install.sh | bash
## for windows only
choco install nvm
# Install node
nvm install 16.14.0
nvm use 16.14.0
## Windows-only step: Replace manually, buggy npm 8.3.1 by npm 8.3.2, see https://github.com/npm/cli/issues/4234#issuecomment-1033732533
# Install packages
npm install yarn
yarn install react-native
# Create a project
npx react-native init ReactUnityApp --template react-native-template-typescript
cd ReactUnityApp
```

# Install

```bash
yarn add @brice-gros/react-native-unity-view
```

Since this project uses the exported data from Unity you will have more configuration steps than a normal React Native module.

## Configuring Unity

To configure Unity to add the exported files to your app we use some build scripts.
And the default configuration expects that you place your Unity Project in the following position relative to our app.

```
.
├── android
├── ios
├── unity
│   └── <Your Unity Project>    // Example: Cube
├── node_modules
├── package.json
└── README.md
```

### Add Unity package
From the package manager menu (`Window` > `Package Manager`), select from the left corner `Add package from git URL`, and enter `com.unity.nuget.newtonsoft-json` and be sure to use version 3.0.1+

### Add Unity scripts

Copy template scripts to your project:
```bash
cp -r node_modules/@brice-gros/react-native-unity-view/template/* ./unity/YourProject/
```
This will add:
 - [Build.cs](template/Assets/Scripts/Editor/Build.cs), controlling the build from the editor
 - [XCodePostBuild.cs](template/Assets/Scripts/Editor/XCodePostBuild.cs), used for ios
 - [UnityMessageManager.cs](template/Assets/Scripts/UnityMessageManager.cs), a script managing the messages between React Native and Unity
 - [Rotate.cs](template/Assets/Scripts/Rotate.cs), a MonoBehavior sample script rotating a game object controllable from react native, and sending back a message to react native


### Player Settings

1. Open your Unity Project
2. Go to Player settings (File => Build Settings => Player Settings)
3. Change `Product Name` to the name of your Xcode project. (`ios/${XcodeProjectName}.xcodeproj`)

#### ◼️ Additional changes for Android Settings

Under `Other Settings` make sure:
 - `Scripting Backend` is set to `IL2CPP`
 - `Api Compatibility Level` is `.NET Standard 2.0`
 - under `Target Architectures`, `ARM64` and `ARMv7` are checked

![Android Configruation](docs/android-player-settings.png)


### Export From Unity

To export, open the `Build Settings` window (`File` > `Build Settings...`).

💡 For a `Development` build with `Script Debugging` enabled, tick the corresponding boxes as usual.

⚠️ **Don't use the `Build` or `Export` button**, and note that using `Switch platform` is not required

👉 **But export** the Unity Project using `ReactNative => Export Android`, or `ReactNative => Export IOS`

![Build dropdown](docs/unity-build.png)

Then the exported artifacts will be placed in a folder called `unityLibrary` inside either the `android` or `ios` folder.

## Configure Native Build

For the react native project to recognize the `unityLibrary` folder which contains the Unity exported project, some changes as to be done for each platform.
### Android Build

To have gradle working properly, some modifications has to be done to the react native project:

1. Add the following to the `android/build.gradle`
```
flatDir {
    dirs "${project(':unityLibrary').projectDir}/libs"
}
```
So it looks like this
```
// [..]
allprojects {
    repositories {
        // [..]
        flatDir {
            dirs "${project(':unityLibrary').projectDir}/libs"
        }
    }
}
```

2. Add these two lines to `android/settings.gradle`

```
include ":unityLibrary"
project(":unityLibrary").projectDir = new File(rootProject.projectDir, './unityLibrary')
```

3. Add this line to `gradle.properties`:
```
unityStreamingAssets=.unity3d
```

### iOS Build **UNTESTED**

1. Open your `ios/{PRODUCT_NAME}.xcworkspace` and add the exported project(`ios/unityLibrary/Unity-Iphone.xcodeproj`) to the workspace root

![Add unity ios project to ReactNative Ios workspace](docs/ios-add-unity-project.png)

2. Select the `Unity-iPhone/Data` folder and change the Target Membership to UnityFramework

![Set Target Membership of Data folder to UnityFramework](docs/ios-set-target-membership.png)

3. Add `UnityFramework.framework` as a library to your Project

![Add UnityFramework to project](docs/ios-add-unityframework.png)

4. Modify `main.m`

```objectivec
#import "UnityUtils.h"

int main(int argc, char * argv[]) {
  @autoreleasepool {
    InitArgs(argc, argv);
    return UIApplicationMain(argc, argv, nil, NSStringFromClass([AppDelegate class]));
  }
}
```

## Build the React Native project

### Android Build
```bash
npx yarn
npx yarn android
```

> 🛠️ _**ANDROID KNOWN ISSUES**_:
>
> On Android, the `local.properties` file from the Unity build folder will be added in your react native project as `android/local.properties` unless it already exists.
>
> _An error message can require you to accept Android sdk manager licenses on the first Android build, this can be done using the following commandline:_
> ```bash
> # From the directory specified by `sdk.dir` in local.properties, run:
> ./tools/bin/sdkmanager.bat --licenses
> ```
>
> Also, since Gradle version >7, React Native project's `android/build.gradle` contains a `ndkVersion` entry which can be incompatible with the `sdk.dir` and `ndk.dir` defined by `local.properties`. In that case, either change it to match the `ndkVersion` from the NDK at `ndk.dir`, or comment out both lines for `sdk.dir` and `ndk.dir` in `local.properties`.

### iOS Build  **UNTESTED**
```bash
npx yarn
npx yarn ios
```

> 🛠️ _**IOS KNOWN ISSUES**_:
>
> In case an error occurs on IOS : `typedef redefinition with different types ('uint8_t' (aka 'unsigned char') vs 'enum clockid_t')`
> Upgrade the versions of Flipper and Flipper-Folly in `ios/Podfile`
> ```pod
>  use_flipper!({ 'Flipper' => '0.134.0', 'Flipper-Folly' => '2.6.10' })
>  post_install do |installer|
>    flipper_post_install(installer)
>  end
> ```
> Then, don't forget to run `pod update` from the `ios` folder afterwards!



# Use in React Native

## UnityView Props

### `onMessage`

Handle received message from Unity in React Native

#### Example:

1. Send message from Unity
```C#
UnityMessageManager.Instance.SendMessageToRN("click");
```

2. Receive message in React Native
```javascript
onMessage(event) {
    console.log('OnUnityMessage: ' + event.nativeEvent.message);    // OnUnityMessage: click
}

render() {
    return (
        <View style={[styles.container]}>
            <UnityView
                style={style.unity}
                onMessage={this.onMessage.bind(this)}
            />
        </View>
    );
}
```

### `onUnityMessage`

[**Recommended**] Handle received **json** message from unity.

```
onUnityMessage(handler) {
    console.log(handler.name); // the message name
    console.log(handler.data); // the message data
    setTimeout(() => {
      // You can also create a callback to Unity.
      handler.send('I am callback!');
    }, 2000);
}

render() {
    return (
        <View style={[styles.container]}>
            <UnityView
                style={style.unity}
                onUnityMessage={this.onMessage.bind(this)}
            />
        </View>
    );
}
```

## UnityModule

```
import { UnityModule } from '@brice-gros/react-native-unity-view';
```

### `isReady(): Promise<boolean>`

Return whether is unity ready.

### `createUnity(): Promise<boolean>`

Manual initialize Unity. Usually Unity is auto created when the first view is added.

### `postMessage(gameObject: string, methodName: string, message: string)`

Send message to unity.

* `gameObject` The Name of GameObject. Also can be a path string.
* `methodName` Method name in GameObject instance.
* `message` The message will post.

Example:

1. Add a message handle method in `MonoBehaviour`.

```C#
public class Rotate : MonoBehaviour {
    void handleMessage(string message) {
		Debug.Log("onMessage:" + message);
	}
}
```

2. Add Unity component to a GameObject.

3. Send message use javascript.

```javascript
onToggleRotate() {
    if (this.unity) {
      // gameobject param also can be 'Cube'.
      UnityModule.postMessage('GameObject/Cube', 'toggleRotate', 'message');
    }
}

render() {
    return (
        <View style={[styles.container]}>
            <UnityView
                ref={(ref) => this.unity = ref}
                style={style.unity}
            />
            <Button label="Toggle Rotate" onPress={this.onToggleRotate.bind(this)} />
        </View>
    );
}

```

### `postMessageToUnityManager(message: string | UnityViewMessage)`

Send message to `UnityMessageManager`.

Same than `postMessage('UnityMessageManager', 'onMessage', message)`

This is the recommended use.

* `message` The message to be posted.

Example:

1. Add a message handle method in C#.

```javascript
void Awake()
{
    UnityMessageManager.Instance.OnMessage += toggleRotate;
}

void onDestroy()
{
    UnityMessageManager.Instance.OnMessage -= toggleRotate;
}

void toggleRotate(string message)
{
    Debug.Log("onMessage:" + message);
    canRotate = !canRotate;
}
```

2. Send message use javascript.

```
onToggleRotate() {
    UnityModule.postMessageToUnityManager('message');
}

render() {
    return (
        <View style={[styles.container]}>
            <UnityView
                ref={(ref) => this.unity = ref}
                style={style.unity}
            />
            <Button label="Toggle Rotate" onPress={this.onToggleRotate.bind(this)} />
        </View>
    );
}
```

### `addMessageListener(listener: (message: string | MessageHandler) => void): number`

Receive string and json message from unity.

### `addStringMessageListener(listener: (message: string) => void): number`

Only receive string message from unity.

### `addUnityMessageListener(listener: (handler: MessageHandler) => void): number`

Only receive json message from unity.

### `pause()`

Pause the unity player.

### `resume()`

Resume the unity player.


## Example Code

```tsx
import React from 'react';
import { StyleSheet, View } from 'react-native';
import UnityView from '@brice-gros/react-native-unity-view';

export default class App extends React.Component {
  render() {
    return (
      <View style={{ flex: 1 }}>
        <View style={{ flex: 1 }}>
          <UnityView style={{ flex: 1 }}/>
        </View>
      </View>
    );
  }
}
```
See [github repository](https://github.com/brice-gros/react-native-unity-view/tree/master/example) for a complete example


# Contributing

```bash
npx yarn
npx yarn typescript
npx yarn prepare
cd example
npx yarn
npx yarn android
```

```bash
npx yarn
npx yarn typescript
npx yarn prepare
npx yarn bootstrap
cd example
npx yarn
npx yarn ios
```

