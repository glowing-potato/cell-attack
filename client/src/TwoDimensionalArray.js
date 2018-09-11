export default class TwoDimensionalArray {
    constructor(width, height, xOff, yOff) {
        this.array = [];
        for (let i = 0; i < height; ++i) {
            this.array.push(new Uint8Array(new ArrayBuffer(width)));
        }
        this.xOff = xOff || 0;
        this.yOff = yOff || 0;
    }

    get(x, y) {
        x -= this.xOff;
        y -= this.yOff;
        if (x >= 0 && x < this.array[0].length && y >= 0 && y < this.array.length) {
            return this.array[y][x];
        } else {
            return 0;
        }
    }

    setRange(leftX, topY, width, height, buffer) {
        leftX -= this.xOff;
        topY -= this.yOff;
        let i = 0;
        for (let y = topY; y < topY + height; ++y) {
            for (let x = leftX; x < leftX + width; ++x) {
                if (x >= 0 && x < this.array[0].length && y >= 0 && y < this.array.length) {
                    this.array[y][x] = buffer[i];
                }
                ++i;
            }
        }
    }
}
