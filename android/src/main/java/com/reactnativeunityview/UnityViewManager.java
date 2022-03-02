package com.reactnativeunityview;

import android.graphics.Color;
import android.view.View;
import android.os.Handler;

import com.facebook.react.bridge.LifecycleEventListener;
import com.facebook.react.bridge.ReactApplicationContext;

import androidx.annotation.NonNull;

import com.facebook.react.uimanager.SimpleViewManager;
import com.facebook.react.uimanager.ThemedReactContext;
import com.facebook.react.uimanager.annotations.ReactProp;

public class UnityViewManager extends SimpleViewManager<View>
  implements LifecycleEventListener, View.OnAttachStateChangeListener {
    public static final String REACT_CLASS = "RNUnityView";
    private ReactApplicationContext context;

    UnityViewManager(ReactApplicationContext context) {
        super();
        this.context = context;
        context.addLifecycleEventListener(this);
    }

    @Override
    @NonNull
    public String getName() {
        return REACT_CLASS;
    }

    @Override
    @NonNull
    public View createViewInstance(ThemedReactContext reactContext) {
      final UnityView view = new UnityView(reactContext);
      view.addOnAttachStateChangeListener(this);

      if (UnityUtils.getPlayer() != null) {
          view.setUnityPlayer(UnityUtils.getPlayer());
      } else {
          UnityUtils.createPlayer(context.getCurrentActivity(), new UnityUtils.CreateCallback() {
              @Override
              public void onReady() {
                  view.setUnityPlayer(UnityUtils.getPlayer());
              }
          });
      }
      return view;
    }

    @ReactProp(name = "color")
    public void setColor(View view, String color) {
        view.setBackgroundColor(Color.parseColor(color));
    }

    //@Override
    public void onDropViewInstance(UnityView view) {
        view.removeOnAttachStateChangeListener(this);
        super.onDropViewInstance(view);
    }

    @Override
    public void onHostResume() {
        if (UnityUtils.isUnityReady()) {
            UnityUtils.getPlayer().resume();
            restoreUnityUserState();
        }
    }

    @Override
    public void onHostPause() {
        if (UnityUtils.isUnityReady()) {
            // Don't use UnityUtils.pause()
            UnityUtils.getPlayer().pause();
        }
    }

    @Override
    public void onHostDestroy() {
        if (UnityUtils.isUnityReady()) {
            UnityUtils.getPlayer().quit();
        }
    }

    private void restoreUnityUserState() {
        // restore the unity player state
        if (UnityUtils.isUnityPaused()) {
            Handler handler = new Handler();
            handler.postDelayed(new Runnable() {
                @Override
                public void run() {
                    if (UnityUtils.getPlayer() != null) {
                        UnityUtils.getPlayer().pause();
                    }
                }
            }, 300); //TODO: 300 is the right one?
        }
    }

    @Override
    public void onViewAttachedToWindow(View v) {
        restoreUnityUserState();
    }

    @Override
    public void onViewDetachedFromWindow(View v) {

    }
 }
