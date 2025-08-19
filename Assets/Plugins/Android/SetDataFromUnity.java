package com.grappus.betblack.dev;

public class SetDataFromUnity {
    private final CommunicationCallBack communicationCallBack;

    public SetDataFromUnity(CommunicationCallBack communicationCallBack){
        this.communicationCallBack = communicationCallBack;
    }
    void sendDataToNativeAndroid(String data) {
        communicationCallBack.onDataReceived(data);
        // Process the received data here as needed
    }

    interface CommunicationCallBack {
        void onDataReceived(String message);
    }
}
