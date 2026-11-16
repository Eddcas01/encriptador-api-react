using Encriptador.Api;

var builder = WebApplication.CreateBuilder(args);

// CORS abierto: es una herramienta local de desarrollo.
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// El cifrador se inyecta por interfaz: se puede cambiar por otro algoritmo
// (o por una librería heredada) sin tocar los endpoints.
builder.Services.AddSingleton<ICifrador, CifradorAes>();

var app = builder.Build();
app.UseCors();

// Sirve el front compilado (Encriptador.Api/wwwroot) si existe.
app.UseDefaultFiles();
app.UseStaticFiles();

static char Sep(string? s) => string.IsNullOrEmpty(s) ? ',' : s[0];

// Aplica la operación al texto completo o a cada elemento conservando el separador.
static string Aplicar(Func<string, string> op, string texto, bool porElemento, char sep)
{
    if (!porElemento) return op(texto);
    if (string.IsNullOrEmpty(texto)) return string.Empty;
    return string.Join(sep, texto.Split(sep).Select(op));
}

app.MapPost("/api/encriptar", (CifrarRequest req, ICifrador cifrador) =>
{
    try
    {
        var salida = Aplicar(t => cifrador.Cifrar(req.Clave ?? "", t), req.Texto ?? "", req.PorElemento, Sep(req.Separador));
        return Results.Ok(new { resultado = salida });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/desencriptar", (CifrarRequest req, ICifrador cifrador) =>
{
    try
    {
        var salida = Aplicar(t => cifrador.Descifrar(req.Clave ?? "", t), req.Texto ?? "", req.PorElemento, Sep(req.Separador));
        return Results.Ok(new { resultado = salida });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

/// <summary>Cuerpo de las peticiones. <c>Clave</c> es la frase de paso; vacía usa la clave por defecto.</summary>
record CifrarRequest(string? Clave, string? Texto, bool PorElemento, string? Separador);
