/**
 * Created by larryw on 5/16/18.
 */
class Buffer {
    constructor(size) {
        this.buffer = _.times(size, _.constant(0));
    }

    feed(x){
        this.buffer.shift();
        this.buffer.push(x);
    }
}

class Smoother extends Buffer {
    constructor(size) {
        super(size);
    }

    get lowpassValue() {
        //return average
        return _.reduce(this.buffer, function(memo, num) { return memo + num}, 0) / this.buffer.length ;
    }
    //get lowPassDeriv() {
    //    return this.buffer[this.buffer.length - 1 ] - this.buffer[this.buffer.length - 4 ];
    //}

    //get peakToPeak(){
    //    var max = Math.max(...this.buffer);
    //    var min = Math.min(...this.buffer);
    //    return Math.abs(min - max);
    //}
}