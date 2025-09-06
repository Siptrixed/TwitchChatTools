using Newtonsoft.Json;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Windows;
using TwitchChatTools.Model.Utils;

namespace TwitchChatTools.Model.WebServer
{
    internal class CustomWebServer
    {
        public static CustomWebServer? Instance { get; private set; }

        private readonly HttpListener _listener = new HttpListener();
        private static Dictionary<string, MethodInfo> _services = new Dictionary<string, MethodInfo>();
        private static Dictionary<string, MethodInfo> _socketHandlers = new Dictionary<string, MethodInfo>();

        private CustomWebServer(params string[] prefixes)
        {
            if (Instance != null) throw new InvalidOperationException("Instance of class CustomWebServer alreay exists");

            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
            }

            // URI prefixes are required eg: "http://localhost:8080/test/"
            if (prefixes == null || prefixes.Length == 0)
            {
                throw new ArgumentException("URI prefixes are required");
            }

            foreach (var s in prefixes)
            {
                _listener.Prefixes.Add(s);
            }
        }
        public static void InitializeAndRunInstance()
        {
            if (Instance != null) throw new InvalidOperationException("Instance of class TwitchChatToolsApp alreay exists");

            Instance = new CustomWebServer("http://localhost:8181/");
            Instance.RunServer();
        }

        private void RunServer() => Task.Run(async () =>
        {
            int trys = 0;
            while (true)
            {
                try
                {
                    InitializeServices();
                    _listener.Start();
                    Run();
                    break;
                }
                catch
                {
                    trys++;
                    await Task.Delay(500);
                    if (trys > 10)
                    {
                        _ = MyAppExt.InvokeUI(() => Application.Current.Shutdown());
                    }
                }
            }
        });

        private static void InitializeServices()
        {
            foreach (var service in Assembly.GetExecutingAssembly().GetTypes())
            {
                var svAttribute = service.GetCustomAttribute<CustomWebServiceAttribute>();
                if (svAttribute != null)
                {
                    var serviceUrl = svAttribute.URL ?? service.Name;
                    if (!string.IsNullOrEmpty(serviceUrl))
                    {
                        serviceUrl += "/";
                    }
                    foreach (var meth in service.GetMethods())
                    {
                        var wmAttribute = meth.GetCustomAttribute<CustomWebMethodAttribute>();
                        if (wmAttribute != null)
                        {
                            var pars = meth.GetParameters();
                            if (pars.Length == 1 &&
                                pars[0].ParameterType == typeof(HttpListenerRequest) &&
                                meth.IsStatic && !meth.IsConstructor)
                            {
                                var methName = string.IsNullOrWhiteSpace(wmAttribute.URL) ? meth.Name : wmAttribute.URL;
                                _services.Add($"{serviceUrl}{methName}", meth);
                            }
                        }
                        var wshAttribute = meth.GetCustomAttribute<CustomWebSocketHandlerAttrubute>();
                        if (wshAttribute != null)
                        {
                            var pars = meth.GetParameters();
                            if (pars.Length == 1 &&
                                pars[1].ParameterType == typeof(CustomWebSocket) &&
                                meth.IsStatic && !meth.IsConstructor)
                            {
                                var methName = string.IsNullOrWhiteSpace(wshAttribute.URL) ? meth.Name : wshAttribute.URL;
                                _socketHandlers.Add($"{serviceUrl}{methName}", meth);
                            }
                        }
                    }
                }
            }
        }

        private static async Task<string> WebRequestу(HttpListenerRequest request)
        {
            var servicename = ParseServiceDestination(request);

            if (string.IsNullOrEmpty(servicename))
            {
                return "<script>alert('Some error in link, please close window');window.close();</script>";
            }

            if (_services.ContainsKey(servicename))
            {
                return await GetResultOfService(_services[servicename].Invoke(null, [request]));
                //return (string)(await _services[servicename].Invoke(null, [request, response]) ?? "");
            }

            return $"<script>alert('Service {servicename} not found, please close window');window.close();</script>";
        }
        private static async Task WebSocketRequestу(HttpListenerRequest request, WebSocket webSocket)
        {
            var servicename = ParseServiceDestination(request);

            if (!string.IsNullOrEmpty(servicename) && _socketHandlers.ContainsKey(servicename))
            {
                _socketHandlers[servicename].Invoke(null, [new CustomWebSocket(webSocket)]);
                return;
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Not found", CancellationToken.None);
        }

        private static async Task<string> GetResultOfService(object? result)
        {
            if (result is Task<string> awaitable)
            {
                result = await awaitable;
            }
            if (result is Task<object> awaitableob)
            {
                result = await awaitableob;
            }

            if (result is string str) return str;
            else return JsonConvert.SerializeObject(result);
        }

        private static string? ParseServiceDestination(HttpListenerRequest request)
        {
            var segs = request.Url?.AbsolutePath.Split('/');
            if (segs == null || segs?.Length < 3)
            {
                return null;
            }

            string servicename = $"{segs![1]}/{segs[2]}";
            return servicename;
        }

        private void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                //Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(async c =>
                        {
                            HttpListenerContext? ctx = c as HttpListenerContext;
                            try
                            {
                                if (ctx == null)
                                {
                                    return;
                                }

                                if (ctx.Request.IsWebSocketRequest)
                                {
                                    HttpListenerWebSocketContext wsCtx = await ctx.AcceptWebSocketAsync(subProtocol: null);
                                    await WebSocketRequestу(ctx.Request, wsCtx.WebSocket);
                                }

                                var rstr = await WebRequestу(ctx.Request);
                                
                                var buf = Encoding.UTF8.GetBytes(rstr);

                                if (string.IsNullOrEmpty(ctx.Response.ContentType))
                                {
                                    if (rstr.StartsWith("{"))
                                    {
                                        ctx.Response.ContentType = "text/json; charset=utf-8";
                                    }
                                    else
                                    {
                                        ctx.Response.ContentType = "text/html; charset=utf-8";
                                    }
                                }

                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch
                            {
                                // ignored
                            }
                            finally
                            {
                                // always close the stream
                                if (ctx != null)
                                {
                                    ctx.Response.OutputStream.Close();
                                }
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }
        private void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
