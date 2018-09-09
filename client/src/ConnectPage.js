import React from "react";
import "./ConnectPage.css";

export default class ConnectPage extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            "address": localStorage.connect_address || "",
            "name": localStorage.connect_name || "",
            "loading": false
        };
        this.handleAddressChange = this.handleAddressChange.bind(this);
        this.handleNameChange = this.handleNameChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
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
                        "loading": false
                    });
                    socket.close();
                } else if (arr[0] === 1) {
                    if (this.props.onConnect) {
                        this.props.onConnect(socket, this.state.name);
                    }
                }
            };
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
                                <div />
                            </div>
                        )}
                    </div>
                    <div className="padding" />
                </div>
                <div className="padding" />
            </div>
        );
    }
}
