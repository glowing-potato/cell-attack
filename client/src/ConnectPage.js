import React from "react";
import "./ConnectPage.css";

const errorCodes = {
    1: "Server crashed!",
    2: "Network error occurred!",
    3: "Unable to connect to server!",
    128: "Username already taken!",
    129: "Game has already started!"
};
const errorTimeout = 5000;

export default class ConnectPage extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            "address": localStorage.connect_address || "",
            "name": localStorage.connect_name || "",
            "loading": false,
            "connected": false,
            "errorCode": props.errorCode,
            "errorClass": props.errorCode ? "" : "pristine"
        };
        this.handleAddressChange = this.handleAddressChange.bind(this);
        this.handleNameChange = this.handleNameChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.handleDismiss = this.handleDismiss.bind(this);
        if (props.errorCode) {
            this.errorTimerId = setTimeout(this.handleDismiss, errorTimeout);
        }
    }

    handleAddressChange(ev) {
        this.setState({
            "address": ev.target.value
        });
        localStorage.connect_address = ev.target.value;
    }

    handleNameChange(ev) {
        this.setState({
            "name": ev.target.value
        });
        localStorage.connect_name = ev.target.value;
    }

    handleSubmit() {
        if (!this.state.loading) {
            this.setState({
                "loading": true
            });
            let socket = new WebSocket(this.state.address, "cell-attack-v0");
            socket.binaryType = "arraybuffer";
            socket.onopen = () => {
                socket.send(this.state.name);
            };
            socket.onmessage = ev => {
                let arr = new Uint8Array(ev.data);
                if (ev.data.byteLength !== 1) {
                    console.log(ev.data.byteLength);
                    console.error("Invalid packet received!");
                } else if (arr[0] & 0x80) {
                    this.setState({
                        "loading": false,
                        "errorCode": arr[0],
                        "errorClass": ""
                    });
                    socket.close();
                } else if (arr[0] === 0) {
                    this.setState({
                        "connected": true
                    });
                } else if (arr[0] === 1) {
                    if (this.props.onConnect) {
                        this.props.onConnect(socket, this.state.name);
                    }
                }
            };
            socket.onclose = () => {
                this.setState({
                    "loading": false,
                    "errorCode": 3,
                    "errorClass": ""
                });
            };
            socket.onerror = ev => {
                console.error(ev);
                this.setState({
                    "loading": false,
                    "errorCode": 2,
                    "errorClass": ""
                });
            };
        }
    }

    handleDismiss() {
        this.setState({
            "errorCode": null
        });
    }

    componentDidMount() {
        if (this.state.errorCode) {
            this.errorTimerId = setTimeout(this.handleDismiss, errorTimeout);
        }
    }

    componentWillUnmount() {
        if (this.errorTimerId) {
            clearTimeout(this.errorTimerId);
        }
    }

    componentDidUpdate(prevProps, prevState) {
        if (this.state.errorCode !== prevState.errorCode) {
            if (this.errorTimerId) {
                clearTimeout(this.errorTimerId);
            }
            this.errorTimerId = setTimeout(this.handleDismiss, errorTimeout);
        }
    }

    render() {
        return (
            <div className="ConnectPage">
                <div className="padding" />
                <div className="col">
                    <div className="padding" />
                    <div className="box">
                        <h1>Connect to Server</h1>
                        <form onSubmit={this.handleSubmit}>
                            <div>
                                <label htmlFor="address">
                                    Address
                                </label>
                                <input className={this.state.loading ? "disabled" : ""} disabled={this.state.loading} type="url" name="address" value={this.state.address} onChange={this.handleAddressChange} />
                            </div>
                            <div>
                                <label htmlFor="name">
                                    Name
                                </label>
                                <input className={this.state.loading ? "disabled" : ""} disabled={this.state.loading} type="text" name="name" value={this.state.name} onChange={this.handleNameChange} />
                            </div>
                            <div className={`btn ${this.state.loading && "disabled"}`} onClick={this.handleSubmit}>
                                Connect
                            </div>
                        </form>
                        {this.state.loading && (
                            <div className="progress">
                                <div className={this.state.connected ? "connected" : "disconnected"} />
                            </div>
                        )}
                    </div>
                    <div className="padding" />
                </div>
                <div className="padding" />
                <div className={`error ${this.state.errorCode ? "active" : this.state.errorClass}`} onClick={this.handleDismiss}>
                    {errorCodes[this.state.errorCode]}
                </div>
            </div>
        );
    }
}
