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

    setRange(leftX, topY, width, height, buffer, off) {
        let minArrayY = topY - this.yOff;
        let maxArrayY = topY + height - this.yOff;
        let minArrayX = leftX - this.xOff;
        let maxArrayX = leftX + width - this.xOff;
        let i = off;
        let y = minArrayY;
        if (y < 0) {
            i -= y * width;
            y = 0;
        }
        for (; y < maxArrayY; ++y) {
            if (y < this.array.length) {
                let j = i;
                let x = minArrayX;
                if (x < 0) {
                    j -= x;
                    x = 0;
                }
                let count = maxArrayX - x;
                if (count > 0) {
                    let src = new Uint8Array(buffer, j, count);
                    this.array[y].set(src, x);
                }
            } else {
                break;
            }
            i += width;
        }
    }
}
