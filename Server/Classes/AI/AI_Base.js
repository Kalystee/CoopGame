let ServerItem = require('../Utils/ServerItem');
let Vector2 = require('../Vector3');
module.exports = class AIBase extends ServerItem{
    constructor(){
        super();
        this.username = "AI_Base";
        this.health = 20;
        this.isDead = false;

    }

    onUpdate(onUpdatePosition){
        //Calculate StateMachine
    }

    dealDamage(amount = Number){
        //Adjust Health
        this.health = this.health - amount;

        //Check if dead
        if(this.health <= 0){
            this.isDead = true;
        }
        return this.isDead;
    }
}