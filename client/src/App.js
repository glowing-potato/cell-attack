import React from "react";
import ConnectPage from "./ConnectPage";
import GameView from "./GameView";

export default class App extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            "connected": false,
            "name": "",
            "cells": 0,
            "score": 0,
            "centerX": 0,
            "centerY": 0,
            "color": 0,
            "leftX": 0,
            "topY": 0,
            "width": 0,
            "height": 0,
            "socket": null
        };
        this.handleConnect = this.handleConnect.bind(this);
        this.handleMessage = this.handleMessage.bind(this);
        this.handleClose = this.handleClose.bind(this);
    }

    handleConnect(socket, name) {
        this.setState({
            "connected": true,
            "name": name,
            "socket": socket
        });
        socket.onmessage = this.handleMessage;
        socket.onclose = this.handleClose;
    }

    handleMessage(ev) {
        let arr, arr2;
        switch (ev.data.byteLength) {
            case 8:
                arr = new Float32Array(ev.data);
                this.setState({
                    "cells": arr[0],
                    "score": arr[1]
                });
                break;
            case 9:
                arr = new Float32Array(ev.data, 1);
                arr2 = new Uint8Array(ev.data);
                this.setState({
                    "centerX": arr[0],
                    "centerY": arr[1],
                    "color": arr2[0] & 0x07
                });
                this.handleViewResize(arr[0] - 32, arr[1] - 32, 64, 64);
                break;
            default:
                if (ev.data.byteLength < 12) {
                    console.error("Invalid packet received!");
                } else {

                }
                break;
        }
    }

    handleClose() {
        this.setState({
            "connected": false
        });
    }

    handleViewResize(leftX, topY, width, height) {
        this.setState({
            "leftX": leftX,
            "topY": topY,
            "width": width,
            "height": height
        });
        let buf = new ArrayBuffer(12);
        let arr = new Int32Array(buf);
        arr[0] = leftX;
        arr[1] = topY;
        arr = new Uint16Array(buf, 8);
        arr[0] = width;
        arr[1] = height;
        this.state.socket.send(buf);
    }

    handleClientDraw(leftX, topY, width, height, data) {
        let buf = new ArrayBuffer(12 + data.length);
        let arr = new Int32Array(buf);
        arr[0] = leftX;
        arr[1] = topY;
        arr = new Uint16Array(buf, 8);
        arr[0] = width;
        arr[1] = height;
        new Uint8Array(buf, 12).set(new Uint8Array(data));
        this.state.socket.send(buf);
    }

    render() {
        return this.state.connected ? (
            <div>
                <GameView name={this.state.name} score={this.state.score} />
            </div>
        ) : (
            <div>
                <ConnectPage onConnect={this.handleConnect} />
            </div>
        );
    }
}
