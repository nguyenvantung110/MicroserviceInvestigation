
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notifications")
    .build();

connection.on("ReceiveOrderStatus", (orderId, status) => {
    console.log(`Order ${orderId} status: ${status}`);
});

connection.start()
    .then(() => console.log("Connected to SignalR"))
    .catch(err => console.error(err));
