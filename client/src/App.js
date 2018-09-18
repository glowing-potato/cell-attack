import React from "react";
import ConnectPage from "./ConnectPage";
import GameView from "./GameView";
import TwoDimensionalArray from "./TwoDimensionalArray";

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
            "socket": null,
            "field": new TwoDimensionalArray(1, 1),
            "fieldNonce": 0,
            "errorCode": null
        };
        this.handleConnect = this.handleConnect.bind(this);
        this.handleMessage = this.handleMessage.bind(this);
        this.handleClose = this.handleClose.bind(this);
        this.handleViewResize = this.handleViewResize.bind(this);
        this.handleError = this.handleError.bind(this);
    }

    handleConnect(socket, name) {
        this.setState({
            "connected": true,
            "name": name,
            "socket": socket,
            "centerX": 32,
            "centerY": 32,
            "width": 64,
            "height": 64,
            "field": new TwoDimensionalArray(64, 64)
        });
        socket.onmessage = this.handleMessage;
        socket.onclose = this.handleClose;
        socket.onerror = this.handleError;
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
                arr2 = new Uint8Array(ev.data, 0);
                this.setState({
                    "centerX": arr[0],
                    "centerY": arr[1],
                    "color": arr2[0] & 0x07
                });
                break;
            default:
                if (ev.data.byteLength < 12) {
                    console.error("Invalid packet received!");
                } else {
                    arr = new Int32Array(ev.data, 0, 8);
                    arr2 = new Uint16Array(ev.data, 8, 4);
                    this.state.field.setRange(arr[0], arr[1], arr2[0], arr2[1], new Uint8Array(ev.data, 12));
                    this.setState({
                        "fieldNonce": this.state.fieldNonce + 1
                    });
                }
                break;
        }
    }

    handleClose() {
        this.setState({
            "connected": false,
            "errorCode": 1
        });
    }

    handleError(ev) {
        console.error(ev);
        this.setState({
            "connected": false,
            "errorCode": 2
        });
    }

    handleViewResize(leftX, topY, width, height) {
        this.setState({
            "leftX": leftX,
            "topY": topY,
            "width": width,
            "height": height,
            "field": new TwoDimensionalArray(width, height, leftX, topY)
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
                <GameView name={this.state.name} score={this.state.score} centerX={this.state.centerX}
                        centerY={this.state.centerY} width={this.state.width} height={this.state.height}
                        leftX={this.state.leftX} topY = {this.state.topY} field={this.state.field}
                        fieldNonce={this.state.fieldNonce} onViewResize={this.handleViewResize} />
            </div>
        ) : (
            <div>
                <ConnectPage onConnect={this.handleConnect} errorCode={this.state.errorCode} />
            </div>
        );
    }
}
