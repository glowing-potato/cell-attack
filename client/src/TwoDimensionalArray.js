export default class TwoDimensionalArray {
    constructor(width, height) {
        this.array = [];
        for (let i = 0; i < height; ++i) {
            this.array.push(new Uint8Array(new ArrayBuffer(width)));
        }
    }

    get(x, y) {
        return this.array[y][x];
    }

    setRange(leftX, topY, width, height, buffer) {
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
