let Connection = require("../Connection");
let ServerItem = require('../Utils/ServerItem');
let Vector3 = require('../Vector3');

module.exports = class LobbyBase {
    constructor(id){
        this.id = id;
        this.connections = [];
        this.serverItems = [];
    }

    onUpdate(){
        let lobby = this;
        let serverItems = lobby.serverItems;


    }

    onEnterLobby(connection = Connection){
        let lobby = this;
        let player = connection.player;

        console.log('Player '+ player.displayerPlayerInformation() +' has enter the lobby('+ lobby.id+')')
        lobby.connections.push(connection);
        player.lobby = lobby.id;
        connection.lobby = lobby;
    }

    onLeaveLobby(connection = Connection){
        let lobby = this;
        let player = connection.player;

        console.log('Player '+ player.displayerPlayerInformation() +' has left the lobby('+ lobby.id+')')
        connection.lobby = undefined;
        let index = lobby.connections.indexOf(connection);
        if(index > -1){
            lobby.connections.splice(index,1);
        }
    }

    onServerSpawn(item = ServerItem, location = Vector3){
        let lobby = this;
        let serverItems = lobby.serverItems;
        let connections = lobby.connections;
        //Set position
        item.position = location;

        serverItems.push(item);
        connections.forEach(connection => {
            connection.socket.emit("serverSpawn",{
                id: item.id,
                name: item.username,
                position : item.position.JSONData()
            })
        })
    }

    onServerUnspawn(item = ServerItem){
        let lobby = this;
        let serverItems = lobby.serverItems;
        let connections = lobby.connections;

        lobby.deleteServerItem(item);

        connections.forEach(connection => {
            connection.socket.emit("serverUnspawn",{
                id: item.id
            })
        })
    }

    deleteServerItem(item = ServerItem){
        let lobby = this;
        let serverItems = lobby.serverItems;
        let index = serverItems.indexOf(item);

        if(index > -1){
            serverItems.splice(index,1);
        }
    }
};