import React from "react";
import "./GameView.css";

export default class GameView extends React.Component {
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
                    <canvas />
                </div>
            </div>
        );
    }
}
