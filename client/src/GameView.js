import React from "react";
import "./GameView.css";

const colors = [
    [ 220, 20, 60 ],
    [ 255, 140, 0 ],
    [ 128, 128, 0 ],
    [ 34, 139, 34 ],
    [ 143, 238, 144 ],
    [ 0, 128, 128 ],
    [ 95, 158, 160 ],
    [ 255, 20, 147 ]
];
const maxWidth = 200;
const minWidth = 1;
const viewMargin = 0.1;

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
            "viewX": this.props.centerX,
            "viewY": this.props.centerY,
            "viewWidth": this.props.width,
            "dragX": null,
            "dragY": null
        };
        this.handleResize = this.handleResize.bind(this);
        this.handleMouseDown = this.handleMouseDown.bind(this);
        this.handleMouseUp = this.handleMouseUp.bind(this);
        this.handleMouseOut = this.handleMouseOut.bind(this);
        this.handleMouseMove = this.handleMouseMove.bind(this);
        this.handleWheel = this.handleWheel.bind(this);
    }

    shouldComponentUpdate(nextProps, nextState) {
        return nextProps.fieldNonce !== this.props.fieldNonce || nextProps.centerX !== this.props.centerX || nextProps.centerY !== this.props.centerY ||
            nextState.viewX !== this.state.viewX || nextState.viewY !== this.state.viewY;
    }

    componentDidUpdate(prevProps) {
        let viewX;
        let viewY;
        let viewHeight = this.state.viewWidth / this.state.width * this.state.height;
        if (prevProps.centerX !== this.props.centerX || prevProps.centerY !== this.props.centerY) {
            viewX = this.props.centerX - this.state.viewWidth / 2;
            viewY = this.props.centerY - viewHeight / 2;
            this.setState({
                "viewX": viewX,
                "viewY": viewY
            });
            this.handleViewResize(viewX, viewY, this.state.viewWidth, this.state.width, this.state.height);
        } else {
            viewX = this.state.viewX;
            viewY = this.state.viewY;
        }
        let ctx = this.canvas.getContext("2d");
        ctx.fillStyle = "#7F7F7F";
        ctx.fillRect(0, 0, this.state.width, this.state.height);
        for (let x = Math.floor(viewX); x < viewX + this.state.viewWidth; ++x) {
            for (let y = Math.floor(viewY); y < viewY + viewHeight; ++y) {
                ctx.fillStyle = colorCodes[this.props.field.get(x, y)];
                ctx.fillRect((x - viewX) * this.state.width / this.state.viewWidth, (y - viewY) * this.state.height / viewHeight, this.state.width / this.state.viewWidth - 1, this.state.height / viewHeight - 1);
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

    handleViewResize(viewX, viewY, viewWidth, width, height) {
        let viewHeight = viewWidth / width * height;
        let marginLowX = viewX - this.props.leftX;
        let marginLowY = viewY - this.props.topY;
        let marginHighX = this.props.leftX + this.props.width - viewX - viewWidth;
        let marginHighY = this.props.topY + this.props.height - viewY - viewHeight;
        if (marginLowX < viewWidth * viewMargin || marginLowY < viewHeight * viewMargin || marginHighX < viewWidth * viewMargin || marginHighY < viewHeight * viewMargin) {
            this.props.onViewResize(Math.floor(viewX - viewWidth), Math.floor(viewY - viewHeight), Math.ceil(3 * viewWidth), Math.ceil(3 * viewHeight));
        }
    }

    handleResize() {
        let width = this.canvasDiv.clientWidth - 10;
        let height = this.canvasDiv.clientHeight - 10;
        this.setState({
            "width": width,
            "height": height
        });
        this.handleViewResize(this.state.viewX, this.state.viewY, this.state.viewWidth, width, height);
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
            this.handleViewResize(viewX, viewY, this.state.viewWidth, this.state.width, this.state.height);
        }
    }

    handleWheel(ev) {
        let viewWidth = this.state.viewWidth * (ev.deltaY > 0 ? 4 / 3 : 3 / 4);
        if (viewWidth < minWidth) {
            if (this.state.viewWidth === minWidth) {
                return;
            }
            viewWidth = minWidth;
        } else if (viewWidth > maxWidth) {
            if (this.state.viewWidth === maxWidth) {
                return;
            }
            viewWidth = maxWidth;
        }
        let bounds = this.canvasDiv.getClientRects()[0];
        let scale = viewWidth / this.state.viewWidth;
        let invScale = 1 - scale;
        let viewX = this.state.viewX + invScale * (ev.clientX - bounds.x) / this.state.width * this.state.viewWidth;
        let viewHeight = this.state.viewWidth / this.state.width * this.state.height;
        let viewY = this.state.viewY + invScale * (ev.clientY - bounds.y) / this.state.height * viewHeight;
        this.setState({
            "viewWidth": viewWidth,
            "viewX": viewX,
            "viewY": viewY
        });
        this.handleViewResize(viewX, viewY, viewWidth, this.state.width, this.state.height);
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
                            onMouseOut={this.handleMouseOut} onWheel={this.handleWheel}>
                        <canvas ref={el => this.canvas = el} width={this.state.width} height={this.state.height} />
                    </div>
                </div>
            </div>
        );
    }
}
