let LobbyBase = require("./LobbyBase");
let GameLobbySettings = require("./GameLobbySetting");
let Connection = require("../Connection");
let LobbyState = require("../Utils/LobbyState");
let AIBase = require('../AI/AI_Base');
let Vector3 = require('../Vector3');

module.exports = class GameLobby extends LobbyBase {
    constructor(id,settings = GameLobbySettings){
        super(id);
        this.settings = settings;
        this.lobbyState = new LobbyState();
        this.bullets = [];
    }

    onUpdate(){
        let lobby = this;

        super.onUpdate();
        //lobby.updateBullets();
    }

    canEnterLobby(connection = Connection){
        let lobby = this;
        let maxPlayerCount = lobby.settings.maxPlayers;
        let currentPlayerCount = lobby.connections.length;

        return currentPlayerCount + 1 <= maxPlayerCount;

    }

    onEnterLobby(connection = Connection){
        let lobby = this;
        let socket = connection.socket;
        super.onEnterLobby(connection);
        lobby.addPlayer(connection);
        if(lobby.connections.length === lobby.settings.maxPlayers){
            console.log("We have enough player we can start the game");
            lobby.lobbyState.currentState = lobby.lobbyState.GAME;
            //lobby.onSpawnAllPlayersIntoGame();
            lobby.onSpawnAIIntoGame();
        }

        let returnData = {
            state: lobby.lobbyState.currentState,
        };
        socket.emit("loadGame");
        socket.emit("lobbyUpdate",returnData);
        socket.broadcast.to(lobby.id).emit("lobbyUpdate",returnData);
        //Handle spawning any server spawned object (loot, bullet etc)
    }

    onLeaveLobby(connection = Connection){
        let lobby = this;
        super.onLeaveLobby(connection);
        lobby.removePlayer(connection);
        //Handle unspawning any server spawned object (loot, bullet etc)
        lobby.onUnspawnAllAIInGame(connection);

    }

    onSpawnAllPlayersIntoGame(){
        let lobby = this;
        let connections = lobby.connections;
        connections.forEach(connection => {
            lobby.addPlayer(connection);
        });
    }

    onSpawnAIIntoGame(){
        let lobby = this;
        console.log("Spawn AI into game");
        lobby.onServerSpawn(new AIBase(),new Vector3(0,2,0));
    }

    onUnspawnAllAIInGame(connection = Connection){
        let lobby = this;
        let serverItems = lobby.serverItems;

        //remove all server items from the client but still leave them in the server others
        serverItems.forEach(serverItem => {
            connection.socket.emit("serverUnspawn", {
                id : serverItem.id
            });
        });
    }

    updateBullets() {
        let lobby = this;
        let bullets = lobby.bullets;
        let connections = lobby.connections;

        bullets.forEach(bullet => {
            let isDestroyed = bullet.onUpdate();

            if(isDestroyed) {
                lobby.despawnBullet(bullet);
            } else {
                /*var returnData = {
                    id: bullet.id,
                    position: {
                        x: bullet.position.x.toString(),
                        y: bullet.position.y.toString()
                    }
                };

                connections.forEach(connection => {
                    connection.socket.emit('updatePosition', returnData);
                });*/
            }
        });
    }

    onFireBullet(connection = Connection, data) {
        let lobby = this;

        let bullet = new Bullet();
        bullet.name = 'Bullet';
        bullet.activator = data.activator;
        bullet.position.x = parseFloat(data.position.x);
        bullet.position.y = parseFloat(data.position.y);
        bullet.direction.x = parseFloat(data.direction.x);
        bullet.direction.y = parseFloat(data.direction.y);

        lobby.bullets.push(bullet);

        var returnData = {
            name: bullet.name,
            id: bullet.id,
            activator: bullet.activator,
            position: {
                x: bullet.position.x.toString(),
                y: bullet.position.y.toString()
            },
            direction: {
                x: bullet.direction.x.toString(),
                y: bullet.direction.y.toString()
            },
            speed: bullet.speed.toString()
        };

        connection.socket.emit('serverSpawn', returnData);
        connection.socket.broadcast.to(lobby.id).emit('serverSpawn', returnData); //Only broadcast to those in the same lobby as us
    }

    onCollisionDestroy(connection = Connection,data){
        let lobby = this;
        let returnBullets = lobby.bullets.filter(bullet => {
            return bullet.id === data.id;
        });

        returnBullets.forEach(bullet =>{
            let playerHit = false;

            lobby.connections.forEach(c =>{
                let player = c.player;
                if(bullet.activator !== player.id) {
                    let distance = bullet.position.Distance(player.position);

                    if (distance < 1.05) {
                        let isDead = player.dealDamage(50); //test deal damage
                        if (isDead) {
                            console.log("Player with id : " + player.id + " has died");
                            let returnData = {
                                id: player.id
                            };
                            c.socket.emit("playerDied", returnData);
                            c.socket.broadcast.to(lobby.id).emit("playerDied", returnData);
                        } else {
                            console.log("Player with id: " + player.id + " has (" + player.health + ") health left");
                        }
                        playerHit = true;
                        lobby.despawnBullet(bullet);
                    }
                }

            });

            if(!playerHit){
                let aiList = lobby.serverItems.filter(item => {
                    return item instanceof AIBase;
                });
                aiList.forEach(ai => {
                    if(bullet.activator !== ai.id){
                        let distance = bullet.position.Distance(ai.position);

                        if(distance < 1.05){
                            let isDead = ai.dealDamage(50);
                            if(isDead){
                                console.log("AI is dead");
                                let returnData = {
                                    id: ai.id,
                                };
                                lobby.connections[0].socket.emit("playerDied",returnData);
                                lobby.connections[0].socket.broadcast.to(lobby.id).emit("playerDied",returnData);
                            }else{
                                console.log("AI have "+ai.health+" HP left");
                            }
                        }
                    }
                });
            }
            if(!playerHit ){
                bullet.isDestroyed = true;
            }

        });

    }

    onAttackSuccess(connection = Connection,data){
        let lobby = this;
        let target = data.targetId;
        let damage = parseInt(data.ammount);
        let initiator = data.initiatorId;
        console.log(initiator+" deal "+damage +" to "+target);
        let aiList = lobby.serverItems.filter(item => {
            return item instanceof AIBase;
        });
        aiList.forEach(ai => {
            if (target === ai.id) {
                let isDead = ai.dealDamage(damage);
                if (isDead) {
                    console.log("AI is dead");
                    let returnData = {
                        id: ai.id,
                    };
                    lobby.connections[0].socket.emit("playerDied", returnData);
                    lobby.connections[0].socket.broadcast.to(lobby.id).emit("playerDied", returnData);
                } else {
                    console.log("AI have " + ai.health + " HP left");
                }
            }
        });
    }
    despawnBullet(bullet = Bullet){

        let lobby = this;
        let bullets = lobby.bullets;
        let connections = lobby.connections;
        console.log("Destroying bullet ("+bullet.id+')');
        var index = bullets.indexOf(bullet);
        if(index > -1){
            bullets.splice(index,1);

            var returnData = {
                id: bullet.id
            };

            connections.forEach(connection => {
                connection.socket.emit('serverUnspawn',returnData);
            });
        }
    }
    addPlayer(connection = Connection){
        let lobby = this;
        let connections = lobby.connections;
        let socket = connection.socket;

        var returnData = {
            id: connection.player.id
        };
        console.log('Add player');
        socket.emit("spawn",returnData);
        socket.broadcast.to(lobby.id).emit("spawn",returnData);

        //Tell myself about everyone else already in the lobby
        connections.forEach(c => {
            if(c.player.id !== connection.player.id){
                socket.emit("spawn",{
                    id: c.player.id
                });
            }
        });
    }

    removePlayer(connection = Connection){
        let lobby = this;

        connection.socket.broadcast.to(lobby.id).emit('disconnected',{
            id: connection.player.id
        })
    }
};