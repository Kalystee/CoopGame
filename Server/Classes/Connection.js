module.exports = class Connection {
    constructor(){
        this.socket;
        this.player;
        this.server;
        this.lobby;
    }

    createEvents(){
        let connection = this;
        let socket = connection.socket;
        let server = connection.server;
        let player = connection.player;

        socket.on("disconnect",function () {
            server.onDisconnected(connection);
        });

        socket.on("joinGame",function () {
            server.onAttemptToJoinGame(connection);
        });

      /*  socket.on("fireBullet",function (data) {
            connection.lobby.onFireBullet(connection,data);
        });

        socket.on("collisionDestroy",function (data) {
            connection.lobby.onCollisionDestroy(connection,data);
        });
*/
        socket.on("updatePosition",function (data) {
            player.position.x = data.position.x;
            player.position.y = data.position.y;
            player.position.z = data.position.z;
            socket.broadcast.to(connection.lobby.id).emit("updatePosition",player);
        });

        socket.on("updateRotation",function (data) {
            player.rotation = data.rotation;

            socket.broadcast.to(connection.lobby.id).emit('updateRotation',player)
        });

        socket.on("takeDamage",function(data){
            connection.lobby.onAttackSuccess(connection,data);
        })
    }
}