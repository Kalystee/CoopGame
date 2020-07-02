var shortID = require('shortid');
var Vector2 = require('./Vector2');

module.exports = class Player {
    constructor(){
        this.username = "Default_Player";
        this.id = shortID.generate();
        this.position = new Vector2();
        this.rotation = new Number(0);
        this.health = new Number(100);
        this.isDead = false;
    }

    displayerPlayerInformation(){
        let player = this;
        return '('+ player.username+" : "+player.id+")";
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
};