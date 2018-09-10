import React from "react";
import "./GameView.css";

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
                let v = this.props.field.get(x, y);
                if (v > 0 && v < 8) {
                    ctx.fillStyle = "#000000";
                } else {
                    ctx.fillStyle = "#FFFFFF";
                }
                ctx.fillRect(x * 1000 / this.props.width, y * 1000 / this.props.height, 1000 / this.props.width - 1, 1000 / this.props.height - 1);
            }
        }
        console.log("Rendering");
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
