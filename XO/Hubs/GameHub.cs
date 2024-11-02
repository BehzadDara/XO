using Microsoft.AspNetCore.SignalR;

namespace XO.Hubs;

public class GameHub : Hub
{
    private static string[,] board = new string[3, 3];
    private static string? playerX;
    private static string? playerO;
    private static bool xTurn = true;

    public async Task JoinGame()
    {
        if (string.IsNullOrEmpty(playerX))
        {
            playerX = Context.ConnectionId;
            await Clients.Caller.SendAsync("SetPlayer", "X");
            await Clients.Caller.SendAsync("Message", "Waiting for another player...");
        }
        else if (string.IsNullOrEmpty(playerO))
        {
            playerO = Context.ConnectionId;
            await Clients.Client(playerX).SendAsync("Message", "Player O joined. You start.");
            await Clients.Caller.SendAsync("SetPlayer", "O");
            await Clients.Caller.SendAsync("Message", "You are Player O. Waiting for Player X's move.");
        }
    }

    public async Task MakeMove(int x, int y, string player)
    {
        if (board[x, y] == null && ((player == "X" && xTurn) || (player == "O" && !xTurn)))
        {
            board[x, y] = player;
            xTurn = !xTurn;
            await Clients.All.SendAsync("ReceiveMove", x, y, player);

            if (CheckWin(player))
            {
                await Task.Delay(100);
                await Clients.Caller.SendAsync("ShowResult", "You Win!");
                await Clients.Others.SendAsync("ShowResult", "You Lose!");
                ResetGame();
            }
            else if (IsBoardFull())
            {
                await Task.Delay(100);
                await Clients.All.SendAsync("ShowResult", "It's a draw!");
                ResetGame();
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Message", "Invalid move or not your turn.");
        }
    }

    private static bool CheckWin(string player)
    {
        for (int i = 0; i < 3; i++)
        {
            if ((board[i, 0] == player && board[i, 1] == player && board[i, 2] == player) ||
                (board[0, i] == player && board[1, i] == player && board[2, i] == player))
            {
                return true;
            }
        }

        if ((board[0, 0] == player && board[1, 1] == player && board[2, 2] == player) ||
            (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player))
        {
            return true;
        }

        return false;
    }

    private static bool IsBoardFull()
    {
        foreach (var cell in board)
        {
            if (cell == null) return false;
        }
        return true;
    }

    private static void ResetGame()
    {
        board = new string[3, 3];
        xTurn = true;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.ConnectionId == playerX) playerX = null;
        if (Context.ConnectionId == playerO) playerO = null;
        await base.OnDisconnectedAsync(exception);
    }
}
