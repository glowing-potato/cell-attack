import React from "react";
import "./GameView.css";

let colors = [
    [ 220, 20, 60 ],
    [ 255, 140, 0 ],
    [ 128, 128, 0 ],
    [ 34, 139, 34 ],
    [ 143, 238, 144 ],
    [ 0, 128, 128 ],
    [ 95, 158, 160 ],
    [ 255, 20, 147 ]
];
let colorCodes = [];
for (let c = 0; c < 8; ++c) {
    for (let v = 0; v < 32; ++v) {
        let a = v === 0 ? 0 : (v + 33) / 64;
        let r = colors[c][0] + (255 - colors[c][0]) * a;
        let g = colors[c][1] + (255 - colors[c][1]) * a;
        let b = colors[c][2] + (255 - colors[c][2]) * a;
        r = Math.floor(r).toString(16);
        if (r.length === 1) {
            r = "0".concat(r);
        }
        g = Math.floor(g).toString(16);
        if (g.length === 1) {
            g = "0".concat(g);
        }
        b = Math.floor(b).toString(16);
        if (b.length === 1) {
            b = "0".concat(b);
        }
        colorCodes[(v << 3) | c] = "#".concat(r, g, b);
    }
}

export default class GameView extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            "width": 1000,
            "height": 1000,
            "viewX": 0,
            "viewY": 0,
            "viewWidth": 64,
            "dragX": null,
            "dragY": null
        };
        this.handleResize = this.handleResize.bind(this);
        this.handleMouseDown = this.handleMouseDown.bind(this);
        this.handleMouseUp = this.handleMouseUp.bind(this);
        this.handleMouseOut = this.handleMouseOut.bind(this);
        this.handleMouseMove = this.handleMouseMove.bind(this);
    }

    shouldComponentUpdate(nextProps) {
        return nextProps.fieldNonce !== this.props.fieldNonce;
    }

    componentDidUpdate() {
        let ctx = this.canvas.getContext("2d");
        ctx.fillStyle = "#7F7F7F";
        ctx.fillRect(0, 0, this.state.width, this.state.height);
        let viewHeight = this.state.viewWidth / this.state.width * this.state.height;
        for (let x = Math.floor(this.state.viewX); x < this.state.viewX + this.state.viewWidth; ++x) {
            for (let y = Math.floor(this.state.viewY); y < this.state.viewY + viewHeight; ++y) {
                ctx.fillStyle = colorCodes[this.props.field.get(x, y)];
                ctx.fillRect((x - this.state.viewX) * this.state.width / this.state.viewWidth, (y - this.state.viewY) * this.state.height / viewHeight, this.state.width / this.state.viewWidth - 1, this.state.height / viewHeight - 1);
            }
        }
    }

    componentDidMount() {
        this.handleResize();
        window.addEventListener("resize", this.handleResize);
    }

    componentWillUnmount() {
        window.removeEventListener("resize", this.handleResize);
    }

    handleResize() {
        let width = this.canvasDiv.clientWidth - 10;
        let height = this.canvasDiv.clientHeight - 10;
        this.setState({
            "width": width,
            "height": height
        });
        let viewHeight = this.state.viewWidth / width * height;
        this.props.onViewResize(Math.floor(this.state.viewX - this.state.viewWidth), Math.floor(this.state.viewY - viewHeight), Math.ceil(3 * this.state.viewWidth), Math.ceil(3 * viewHeight));
    }

    handleMouseDown(ev) {
        this.setState({
            "dragX": ev.clientX,
            "dragY": ev.clientY
        });
    }

    handleMouseUp(ev) {
        this.setState({
            "dragX": null,
            "dragY": null
        });
    }

    handleMouseOut(ev) {
        this.handleMouseUp(ev);
    }

    handleMouseMove(ev) {
        if (this.state.dragX !== null && this.state.dragY !== null) {
            let viewX = this.state.viewX - (ev.clientX - this.state.dragX) * this.state.viewWidth / this.state.width;
            let viewY = this.state.viewY - (ev.clientY - this.state.dragY) * this.state.viewWidth / this.state.width * this.state.height / this.state.height;
            this.setState({
                "dragX": ev.clientX,
                "dragY": ev.clientY,
                "viewX": viewX,
                "viewY": viewY
            });
            let viewHeight = this.state.viewWidth / this.state.width * this.state.height;
            this.props.onViewResize(Math.floor(viewX - this.state.viewWidth), Math.floor(viewY - viewHeight), Math.ceil(3 * this.state.viewWidth), Math.ceil(3 * viewHeight));
        }
    }

    render() {
        return (
            <div className="GameView">
                <div className="titlebar">
                    <div className="name">
                        {this.props.name}
                    </div>
                    <div className="score">
                        {this.props.score}
                    </div>
                </div>
                <div className="container">
                    <div className="sidebar">

                    </div>
                    <div className="canvas" ref={el => this.canvasDiv = el} onMouseDown={this.handleMouseDown}
                            onMouseMove={this.handleMouseMove} onMouseUp={this.handleMouseUp}
                            onMouseOut={this.handleMouseOut}>
                        <canvas ref={el => this.canvas = el} width={this.state.width} height={this.state.height} />
                    </div>
                </div>
            </div>
        );
    }
}
