var shortID = require('shortid');
var Vector3 = require('./Vector3');

module.exports = class Player {
    constructor(){
        this.username = "Default_Player";
        this.id = shortID.generate();
        this.lobby = 0;
        this.position = new Vector3();
        this.rotation = "";
        this.health = new Number(100);
        this.isDead = false;
    }

    displayerPlayerInformation(){
        let player = this;
        return '('+ player.username+" : "+player.id+")";
    }

   /* dealDamage(amount = Number){
        //Adjust Health
        this.health = this.health - amount;

        //Check if dead
        if(this.health <= 0){
            this.isDead = true;
        }
        return this.isDead;
    }*/
};