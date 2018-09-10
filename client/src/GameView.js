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
    shouldComponentUpdate(nextProps) {
        return nextProps.fieldNonce !== this.props.fieldNonce;
    }

    componentDidUpdate() {
        let ctx = this.canvas.getContext("2d");
        ctx.fillStyle = "#7F7F7F";
        ctx.fillRect(0, 0, 1000, 1000);
        for (let x = this.props.leftX; x < this.props.leftX + this.props.width; ++x) {
            for (let y = this.props.topY; y < this.props.topY + this.props.height; ++y) {
                ctx.fillStyle = colorCodes[this.props.field.get(x, y)];
                ctx.fillRect(x * 1000 / this.props.width, y * 1000 / this.props.height, 1000 / this.props.width - 1, 1000 / this.props.height - 1);
            }
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
                    <canvas ref={el => this.canvas = el} width="1000" height="1000" />
                </div>
            </div>
        );
    }
}
