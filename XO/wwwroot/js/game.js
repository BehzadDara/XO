document.addEventListener("DOMContentLoaded", () => {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .build();

    let playerSymbol = null;

    connection.start()
        .then(() => {
            connection.invoke("JoinGame");
        })
        .catch(err => console.error("Connection error or invoke error:", err));

    connection.on("SetPlayer", (symbol) => {
        playerSymbol = symbol;
        document.getElementById("status").innerText = `You are Player ${symbol}`;
    });

    connection.on("Message", (message) => {
        document.getElementById("status").innerText = message;
    });

    connection.on("ReceiveMove", (x, y, symbol) => {
        const cell = document.querySelector(`.cell[data-x="${x}"][data-y="${y}"]`);
        cell.innerText = symbol;
    });

    connection.on("ShowResult", (result) => {
        alert(result);
        document.querySelectorAll(".cell").forEach(cell => cell.innerText = "");
    });

    document.querySelectorAll(".cell").forEach(cell => {
        cell.addEventListener("click", () => {
            const x = cell.getAttribute("data-x");
            const y = cell.getAttribute("data-y");

            connection.invoke("MakeMove", parseInt(x), parseInt(y), playerSymbol)
                .catch(err => console.error(err.toString()));
        });
    });
});
