import { useState } from 'react'

export default function App() {
  const [clave, setClave] = useState('')
  const [texto, setTexto] = useState('')
  const [porElemento, setPorElemento] = useState(false)
  const [separador, setSeparador] = useState(',')
  const [resultado, setResultado] = useState('')
  const [error, setError] = useState('')
  const [cargando, setCargando] = useState(false)
  const [copiado, setCopiado] = useState(false)

  async function llamar(endpoint) {
    setError('')
    setCopiado(false)
    setCargando(true)
    try {
      const resp = await fetch(`/api/${endpoint}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          clave,
          texto,
          porElemento,
          separador: separador || ','
        })
      })
      const data = await resp.json()
      if (!resp.ok) {
        setError(data.error || 'Error desconocido')
        setResultado('')
      } else {
        setResultado(data.resultado ?? '')
      }
    } catch (e) {
      setError('No se pudo contactar la API (¿está corriendo en el puerto 5080?). ' + e.message)
      setResultado('')
    } finally {
      setCargando(false)
    }
  }

  function copiar() {
    navigator.clipboard.writeText(resultado).then(() => {
      setCopiado(true)
      setTimeout(() => setCopiado(false), 1500)
    })
  }

  function usarResultadoComoEntrada() {
    setTexto(resultado)
    setResultado('')
  }

  return (
    <div className="wrap">
      <h1>Encriptador</h1>
      <p className="sub">
        Cifra y descifra cualquier cadena (o conjunto de cadenas separadas por coma o cualquier
        carácter) con AES-256-GCM a través de una API .NET 8.
      </p>

      <label className="campo">
        <span>Clave (opcional; vacía usa la clave por defecto)</span>
        <input
          type="password"
          value={clave}
          onChange={(e) => setClave(e.target.value)}
          placeholder="(clave por defecto)"
          autoComplete="off"
        />
      </label>

      <label className="campo">
        <span>Texto</span>
        <textarea
          rows={5}
          value={texto}
          onChange={(e) => setTexto(e.target.value)}
          placeholder="Texto a cifrar, o el hexadecimal a descifrar"
        />
      </label>

      <div className="opciones">
        <label className="check">
          <input
            type="checkbox"
            checked={porElemento}
            onChange={(e) => setPorElemento(e.target.checked)}
          />
          <span>Cifrar cada elemento por separado</span>
        </label>
        {porElemento && (
          <label className="sepField">
            <span>Separador</span>
            <input
              type="text"
              maxLength={1}
              value={separador}
              onChange={(e) => setSeparador(e.target.value)}
            />
          </label>
        )}
      </div>

      <div className="botones">
        <button onClick={() => llamar('encriptar')} disabled={cargando}>
          Encriptar
        </button>
        <button className="secundario" onClick={() => llamar('desencriptar')} disabled={cargando}>
          Desencriptar
        </button>
      </div>

      {error && <div className="error">{error}</div>}

      {resultado !== '' && (
        <div className="resultado">
          <div className="resultadoHead">
            <span>Resultado</span>
            <div className="resultadoAcciones">
              <button className="mini" onClick={copiar}>
                {copiado ? '¡Copiado!' : 'Copiar'}
              </button>
              <button className="mini" onClick={usarResultadoComoEntrada}>
                Usar como entrada
              </button>
            </div>
          </div>
          <pre>{resultado}</pre>
        </div>
      )}
    </div>
  )
}
