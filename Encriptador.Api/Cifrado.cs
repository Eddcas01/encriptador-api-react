using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Encriptador.Api;

/// <summary>Contrato del cifrador. Permite cambiar el algoritmo sin tocar la API.</summary>
public interface ICifrador
{
    /// <summary>Cifra <paramref name="texto"/> y devuelve una cadena hexadecimal.</summary>
    string Cifrar(string clave, string texto);

    /// <summary>Descifra una cadena hexadecimal generada por <see cref="Cifrar"/>.</summary>
    string Descifrar(string clave, string hex);
}

/// <summary>
/// Implementación de demostración con AES-256-GCM (cifrado autenticado).
/// La clave se deriva de la frase de paso con PBKDF2-SHA256 y una sal aleatoria por mensaje.
/// Formato de salida (hex): sal(16) | nonce(12) | tag(16) | texto cifrado.
/// </summary>
public sealed class CifradorAes : ICifrador
{
    private const int TamSal = 16, TamNonce = 12, TamTag = 16, TamClave = 32, Iteraciones = 100_000;

    // Solo para la demo: en un entorno real la clave vendría de configuración segura (Key Vault, variables de entorno...).
    private readonly string _clavePorDefecto;

    public CifradorAes(IConfiguration config) =>
        _clavePorDefecto = config["Cifrado:ClavePorDefecto"] ?? "clave-de-demostracion";

    public string Cifrar(string clave, string texto)
    {
        var sal = RandomNumberGenerator.GetBytes(TamSal);
        var nonce = RandomNumberGenerator.GetBytes(TamNonce);
        var plano = Encoding.UTF8.GetBytes(texto);
        var cifrado = new byte[plano.Length];
        var tag = new byte[TamTag];

        using (var aes = new AesGcm(Derivar(clave, sal), TamTag))
            aes.Encrypt(nonce, plano, cifrado, tag);

        return Convert.ToHexString([.. sal, .. nonce, .. tag, .. cifrado]);
    }

    public string Descifrar(string clave, string hex)
    {
        byte[] datos;
        try { datos = Convert.FromHexString(hex.Trim()); }
        catch (FormatException) { throw new ArgumentException("El texto cifrado debe ser hexadecimal."); }
        if (datos.Length < TamSal + TamNonce + TamTag)
            throw new ArgumentException("El texto cifrado está incompleto.");

        var sal = datos.AsSpan(0, TamSal);
        var nonce = datos.AsSpan(TamSal, TamNonce);
        var tag = datos.AsSpan(TamSal + TamNonce, TamTag);
        var cifrado = datos.AsSpan(TamSal + TamNonce + TamTag);
        var plano = new byte[cifrado.Length];

        try
        {
            using var aes = new AesGcm(Derivar(clave, sal.ToArray()), TamTag);
            aes.Decrypt(nonce, cifrado, tag, plano);
        }
        catch (AuthenticationTagMismatchException)
        {
            throw new ArgumentException("Clave incorrecta o datos alterados.");
        }
        return Encoding.UTF8.GetString(plano);
    }

    private byte[] Derivar(string clave, byte[] sal) =>
        Rfc2898DeriveBytes.Pbkdf2(string.IsNullOrEmpty(clave) ? _clavePorDefecto : clave,
                                  sal, Iteraciones, HashAlgorithmName.SHA256, TamClave);
}
