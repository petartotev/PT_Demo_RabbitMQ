using RPC.RpcClient;

while (true)
{
    var rpcClient = new RpcClient();

    Console.Write("Enter a number from 1 to 50: ");

    var input = Console.ReadLine();

    Console.WriteLine($"[x] Requesting Fibonacci({input})");

    var response = rpcClient.Call(input);

    Console.WriteLine("[.] Got '{0}'", response);

    rpcClient.Close();
}