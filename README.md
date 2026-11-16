# Encriptador · API .NET + React

Herramienta web para **cifrar y descifrar cadenas de texto**, por ejemplo cadenas de conexión o listas de valores
separados por coma, con **AES-256-GCM**. Tiene un backend en **ASP.NET Core (.NET 10, Minimal API)** y un
frontend en **React + Vite**.

> El proyecto original envolvía una librería de cifrado heredada (.NET Framework 1.1) para poder usarla desde
> .NET moderno. Esta versión pública reemplaza esa librería por una implementación AES propia detrás de una
> interfaz, así que el diseño es el mismo y se puede cambiar el algoritmo sin tocar la API.

## Características

- **AES-256-GCM** (cifrado autenticado): si los datos se alteran o la clave es incorrecta, se detecta.
- Clave derivada con **PBKDF2-SHA256** (100 000 iteraciones) y **sal aleatoria por mensaje**: el mismo texto
  nunca produce el mismo cifrado.
- Salida en **hexadecimal**: `sal(16) | nonce(12) | tag(16) | datos`.
- Modo **"por elemento"**: cifra cada valor de una lista por separado y conserva el separador (`,`, `;`, `|`…).
- Cifrador inyectado por **interfaz (`ICifrador`)** con el contenedor de dependencias de ASP.NET Core.
- El frontend compila dentro de `wwwroot`, así que en producción todo se sirve desde un solo proceso .NET.

## Estructura

```
├─ Encriptador.Api/        API .NET 10 (Minimal API)
│  ├─ Program.cs           Endpoints /api/encriptar y /api/desencriptar
│  └─ Cifrado.cs           ICifrador + CifradorAes (AES-256-GCM + PBKDF2)
└─ frontend/               React 18 + Vite
```

## Cómo ejecutar

Requisitos: [.NET SDK 10](https://dotnet.microsoft.com/download) y Node.js 18 o superior.

```bash
# 1. API (http://localhost:5080)
dotnet run --project Encriptador.Api

# 2. Front en modo desarrollo (http://localhost:5173, reenvía /api a la API)
cd frontend
npm install
npm run dev
```

Para producción, compila el front dentro de la API y sirve todo desde .NET:

```bash
cd frontend && npm install && npm run build
cd .. && dotnet run --project Encriptador.Api   # abre http://localhost:5080
```

## API

`POST /api/encriptar` · `POST /api/desencriptar`

```json
{ "clave": "mi-frase", "texto": "valor1,valor2", "porElemento": true, "separador": "," }
```

Respuesta: `{ "resultado": "..." }` o, si hay error, `{ "error": "..." }` con HTTP 400.

Si `clave` va vacía se usa `Cifrado:ClavePorDefecto` de la configuración (por ejemplo `appsettings.json` o
variables de entorno). **No uses la clave de demostración en un entorno real.**

## Tecnologías

C# · ASP.NET Core Minimal API · System.Security.Cryptography (AesGcm, PBKDF2) · React · Vite
