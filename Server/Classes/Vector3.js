module.exports = class Vector3{
    constructor(x=0, y=0,z=0){
        this.x = x;
        this.y = y;
        this.z = z;
    }

    Magnitude(){
        return Math.sqrt((this.x * this.x) + (this.y*this.y)+ (this.z*this.z));
    }

    Normalized(){
        let mag = this.Magnitude();
        return new Vector3(this.x/ mag, this.y/ mag,this.z/ mag);
    }

    Distance(OtherVect = Vector3){
        let direction = new Vector3();
        direction.x = OtherVect.x - this.x;
        direction.y = OtherVect.y - this.y;
        direction.z = OtherVect.z - this.z;
        return direction.Magnitude();
    }
    ConsoleOutput(){
        return '{' + this.x+','+this.y+','+this.z+'}';
    }

    JSONData(){
        return {
            x: this.x,
            y: this.y,
            z: this.z
        }
    }
}