using LearnAI.Agents.Extensions;
using LearnAI.Chat.Extensions;
using LearnAI.Gateway.Extensions;
using LearnAI.Kernel.Extensions;
using LearnAI.MCP.Extensions;
using LearnAI.MCP.Models;
using LearnAI.Tools.Extensions;
using LearnAI.Azure.Extensions;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

const string allowSpecificOrigins = "_allowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:55470")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var mcpServerInfo = new McpServerInfo
{
    Name = "LearnAI.Core.McpServer",
    Version = "1.0.0",
    ProtocolVersion = "2024-11-05"
};

builder.Services.AddAiChatServices();
builder.Services.AddAIGateway(builder.Configuration);
builder.Services.AddLearnAIToolsEngine();
builder.Services.AddSemanticKernelInfrastructure();
builder.Services.AddAgentInfrastructure();
builder.Services.AddMcpInfrastructure(mcpServerInfo);
builder.Services.AddAzureInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(allowSpecificOrigins);
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<LearnAI.MCP.Interfaces.IMcpClient>() as LearnAI.MCP.Client.McpClient;
    var server = scope.ServiceProvider.GetRequiredService<LearnAI.MCP.Interfaces.IMcpServer>() as LearnAI.MCP.Server.McpServer;
    var toolRegistry = scope.ServiceProvider.GetRequiredService<LearnAI.MCP.Interfaces.IToolRegistry>() as LearnAI.MCP.Tools.ToolRegistry;

    if (client != null && server != null && toolRegistry != null)
    {
        toolRegistry.RegisterTool(
            LearnAI.MCP.Tools.CalculatorTool.GetMetadata(),
            LearnAI.MCP.Tools.CalculatorTool.HandleExecutionAsync
        );
        toolRegistry.RegisterTool(
            LearnAI.MCP.Tools.WeatherTool.GetMetadata(),
            LearnAI.MCP.Tools.WeatherTool.HandleExecutionAsync
        );

        var clientToServerPipe = new DualChannelPipe();
        var serverToClientPipe = new DualChannelPipe();

        client.RegisterServerTransport("LocalMathServer", clientToServerPipe.WriterSide, serverToClientPipe.ReaderSide);

        var cts = new System.Threading.CancellationTokenSource();
        _ = server.StartAsync(cts.Token);
        _ = client.StartListeningAsync("LocalMathServer", cts.Token);

        _ = server.ProcessRequestStreamAsync(clientToServerPipe.ReaderSide, serverToClientPipe.WriterSide, cts.Token);
    }
}

app.Run();

public class DualChannelPipe
{
    private readonly Channel<byte[]> _channel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = true
    });

    public Stream WriterSide => new ChannelWriteStream(_channel.Writer);
    public Stream ReaderSide => new ChannelReadStream(_channel.Reader);

    private class ChannelWriteStream : Stream
    {
        private readonly ChannelWriter<byte[]> _writer;
        public ChannelWriteStream(ChannelWriter<byte[]> writer) => _writer = writer;

        public override void Write(byte[] buffer, int offset, int count)
        {
            var data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);
            _writer.TryWrite(data);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);
            await _writer.WriteAsync(data, cancellationToken);
        }

        public override void Flush() { }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    private class ChannelReadStream : Stream
    {
        private readonly ChannelReader<byte[]> _reader;
        private byte[]? _currentRemainder;
        private int _remainderOffset;

        public ChannelReadStream(ChannelReader<byte[]> reader) => _reader = reader;

        public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_currentRemainder == null || _remainderOffset >= _currentRemainder.Length)
            {
                if (!await _reader.WaitToReadAsync(cancellationToken)) return 0;
                if (!_reader.TryRead(out _currentRemainder) || _currentRemainder == null) return 0;
                _remainderOffset = 0;
            }

            int available = _currentRemainder.Length - _remainderOffset;
            int toCopy = Math.Min(available, count);
            Buffer.BlockCopy(_currentRemainder, _remainderOffset, buffer, offset, toCopy);
            _remainderOffset += toCopy;
            return toCopy;
        }

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}