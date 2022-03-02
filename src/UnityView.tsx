import * as React from 'react'
import { requireNativeComponent, UIManager, Platform, View, ViewProps } from 'react-native'
import MessageHandler from './MessageHandler'
import { UnityModule } from './UnityModule'
import { Component } from 'react'

export interface UnityViewProps extends ViewProps {
    /**
     * Receive string message from unity.
     */
    onMessage?: (message: string) => void;
    /**
     * Receive unity message from unity.
     */
    onUnityMessage?: (handler: MessageHandler) => void;

    children?: React.ReactNode
}

let NativeUnityView : any

class UnityView extends Component<UnityViewProps> {

    state = {
        handle: 0
    }

    componentDidMount(): void {
        const { onUnityMessage, onMessage } = this.props
        this.setState({
            handle: UnityModule.addMessageListener(message => {
                if (onUnityMessage && message instanceof MessageHandler) {
                    onUnityMessage(message)
                }
                if (onMessage && typeof message === 'string') {
                    onMessage(message)
                }
            })
        })
    }

    componentWillUnmount(): void {
        UnityModule.removeMessageListener(this.state.handle)
    }

    render() {
        const { props } = this
        return (
            <View {...props}>
            <NativeUnityView
                style={{ position: 'absolute', left: 0, right: 0, top: 0, bottom: 0 }}
                onUnityMessage={props.onUnityMessage}
                onMessage={props.onMessage}
            >
            </NativeUnityView>
            {props.children}
        </View>
        )
    }
}

const LINKING_ERROR =
  `The package '@brice-gros/react-native-unity-view' doesn't seem to be linked. Make sure: \n\n` +
  Platform.select({ ios: "- You have run 'pod install'\n", default: '' }) +
  '- You rebuilt the app after installing the package\n' +
  '- You are not using Expo managed workflow, Expo does not support native libraries\n';

const ComponentName = 'RNUnityView';
NativeUnityView =
  UIManager.getViewManagerConfig(ComponentName) != null
    ? requireNativeComponent<UnityViewProps>(ComponentName)
    : () => {
        throw new Error(LINKING_ERROR);
      };


export default UnityView;
