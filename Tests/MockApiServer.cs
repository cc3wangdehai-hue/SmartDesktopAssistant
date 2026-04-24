using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartDesktopAssistant.Tests
{
    /// <summary>
    /// Mock API Server for testing API robustness
    /// Simulates various API responses: success, delay, error, timeout
    /// </summary>
    public class MockWeatherApiServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly int _port;
        private readonly CancellationTokenSource _cts;
        private Task? _serverTask;
        private int _responseDelayMs = 0;
        private int _statusCode = 200;
        private string _responseMode = "success";

        public MockWeatherApiServer(int port = 8765)
        {
            _port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{_port}/");
            _cts = new CancellationTokenSource();
        }

        public string BaseUrl => $"http://localhost:{_port}";

        public void SetResponseMode(string mode)
        {
            _responseMode = mode switch
            {
                "success" => "success",
                "delay" => "delay",
                "timeout" => "timeout",
                "error404" => "error404",
                "error500" => "error500",
                "empty" => "empty",
                "malformed" => "malformed",
                _ => "success"
            };
        }

        public void SetResponseDelay(int delayMs)
        {
            _responseDelayMs = delayMs;
        }

        public void SetStatusCode(int code)
        {
            _statusCode = code;
        }

        public void Start()
        {
            try
            {
                _listener.Start();
                _serverTask = HandleRequestsAsync(_cts.Token);
                Console.WriteLine($"Mock API Server started on port {_port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start mock server: {ex.Message}");
            }
        }

        private async Task HandleRequestsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = ProcessRequestAsync(context, token);
                }
                catch (HttpListenerException) when (token.IsCancellationRequested)
                {
                    // Expected during shutdown
                    break;
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context, CancellationToken token)
        {
            var response = context.Response;
            var path = context.Request.Url?.AbsolutePath ?? "";

            try
            {
                // Apply delay
                if (_responseDelayMs > 0)
                {
                    await Task.Delay(_responseDelayMs, token);
                }

                // Handle different response modes
                var (statusCode, content) = _responseMode switch
                {
                    "success" => GetSuccessResponse(path),
                    "delay" => GetSuccessResponse(path),
                    "timeout" => (408, "{\"error\": \"Request Timeout\"}"),
                    "error404" => (404, "{\"error\": \"Not Found\"}"),
                    "error500" => (500, "{\"error\": \"Internal Server Error\"}"),
                    "empty" => (200, "{}"),
                    "malformed" => (200, "{this is not valid json}"),
                    _ => GetSuccessResponse(path)
                };

                response.StatusCode = statusCode;
                response.ContentType = "application/json";
                
                var buffer = Encoding.UTF8.GetBytes(content);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, token);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                var errorBuffer = Encoding.UTF8.GetBytes($"{{\"error\": \"{ex.Message}\"}}");
                await response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length, token);
            }
            finally
            {
                response.Close();
            }
        }

        private (int statusCode, string content) GetSuccessResponse(string path)
        {
            if (path.Contains("search"))
            {
                // Geocoding API response
                var response = new
                {
                    results = new[]
                    {
                        new
                        {
                            name = "Beijing",
                            country = "China",
                            latitude = 39.9042,
                            longitude = 116.4074,
                            timezone = "Asia/Shanghai"
                        }
                    }
                };
                return (200, JsonConvert.SerializeObject(response));
            }
            else if (path.Contains("forecast"))
            {
                // Weather API response
                var response = new
                {
                    current = new
                    {
                        temperature_2m = 25.5,
                        weather_code = 0,
                        wind_speed_10m = 10.2
                    },
                    daily = new
                    {
                        temperature_2m_max = new[] { 28.0, 27.0, 26.0, 25.0, 26.0, 27.0, 28.0 },
                        temperature_2m_min = new[] { 18.0, 17.0, 16.0, 15.0, 16.0, 17.0, 18.0 },
                        weather_code = new[] { 0, 1, 2, 3, 0, 1, 2 },
                        time = new[] { "2024-01-01", "2024-01-02", "2024-01-03", "2024-01-04", "2024-01-05", "2024-01-06", "2024-01-07" }
                    }
                };
                return (200, JsonConvert.SerializeObject(response));
            }

            return (404, "{\"error\": \"Unknown endpoint\"}");
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
            _serverTask?.Wait(TimeSpan.FromSeconds(5));
            Console.WriteLine("Mock API Server stopped");
        }

        public void Dispose()
        {
            Stop();
            _listener.Close();
            _cts.Dispose();
        }
    }

    /// <summary>
    /// Test scenarios using mock API
    /// </summary>
    public class MockApiTestScenarios
    {
        public static async Task TestSuccessResponse()
        {
            using var server = new MockWeatherApiServer();
            server.SetResponseMode("success");
            server.Start();

            // Test your code pointing to server.BaseUrl
            Console.WriteLine($"Testing with: {server.BaseUrl}");
        }

        public static async Task TestSlowNetwork()
        {
            using var server = new MockWeatherApiServer();
            server.SetResponseMode("delay");
            server.SetResponseDelay(5000); // 5 second delay
            server.Start();

            Console.WriteLine("Testing slow network (5s delay)...");
        }

        public static async Task TestApiError()
        {
            using var server = new MockWeatherApiServer();
            server.SetResponseMode("error500");
            server.Start();

            Console.WriteLine("Testing API error (500)...");
        }

        public static async Task TestMalformedResponse()
        {
            using var server = new MockWeatherApiServer();
            server.SetResponseMode("malformed");
            server.Start();

            Console.WriteLine("Testing malformed JSON response...");
        }
    }
}
