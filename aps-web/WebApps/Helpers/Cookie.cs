using Microsoft.JSInterop;

namespace ReportsWebApp.Helpers;

public class Cookie
{
    private IDictionary<string, string>? _values;
    private readonly IJSRuntime _js;
    public Cookie(IJSRuntime js) => _js = js;
    public async Task<string> Get(string name) => (await Read()).TryGetValue(name, out var val) ? val : string.Empty;
    public async Task Set(string name, string val, int? days = null)
    {
        await Write(name, val, days);
        (await Read())[name] = val;
    }
    private async Task<IDictionary<string, string>> Read() =>
        _values ??= (await _js.InvokeAsync<string>("Cookie.Read"))
                    .Split(";")
                    .Select(nv => nv.Split("="))
                    .ToDictionary(nv => nv.First().Trim(), nv => nv.Last().Trim());
    private async Task Write(string name, string val, int? days) => await _js.InvokeAsync<string>("Cookie.Write", name, val, days);

}