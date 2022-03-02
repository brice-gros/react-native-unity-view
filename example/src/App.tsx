import React, { Component } from 'react';
import { StyleSheet, View, Button, Alert } from 'react-native';
import { Colors } from 'react-native/Libraries/NewAppScreen';

import UnityView, { UnityModule } from '@brice-gros/react-native-unity-view';


export default class App extends Component {
  App() {
    const [count, _setClickCount] = React.useState(0)
    console.log(count)
  }

  onUnityMessage(handler:any) {
    console.log({handler})
  }

  onClick() {
      UnityModule.postMessageToUnityManager({
                                                name: 'ToggleRotate',
                                                data: '',
                                                callBack: (data) => {
                                                    Alert.alert('Tip', JSON.stringify(data))
                                                }
                                            });
  }


  render() {
      return (
        <View  style={{ flex: 1 }}>
          <View style={{ flex: 1 }}>
            <UnityView
                style={{ flex: 1 }}
                onMessage={this.onUnityMessage}
                onUnityMessage={this.onUnityMessage}
            />
          </View>
          <Button title="Toggle rotation" onPress={this.onClick}/>
        </View>
      );
  }
}

StyleSheet.create({
                                   scrollView: {
                                     backgroundColor: Colors.lighter,
                                   },
                                   engine: {
                                     position: 'absolute',
                                     right: 0,
                                   },
                                   body: {
                                     backgroundColor: Colors.white,
                                   },
                                   sectionContainer: {
                                     marginTop: 32,
                                     paddingHorizontal: 24,
                                   },
                                   sectionTitle: {
                                     fontSize: 24,
                                     fontWeight: '600',
                                     color: Colors.black,
                                   },
                                   sectionDescription: {
                                     marginTop: 8,
                                     fontSize: 18,
                                     fontWeight: '400',
                                     color: Colors.dark,
                                   },
                                   highlight: {
                                     fontWeight: '700',
                                   },
                                   footer: {
                                     color: Colors.dark,
                                     fontSize: 12,
                                     fontWeight: '600',
                                     padding: 4,
                                     paddingRight: 12,
                                     textAlign: 'right',
                                   },
                                 });
