import numpy as np
import sounddevice as sd
import requests
from datetime import datetime

API_URL = "https://localhost:5000/api/asistencia/registrar"


def enviar_asistencia_a_csharp(id_estudiante, id_sesion):
    payload = {
        "idEstudiante": id_estudiante,
        "idSesion": id_sesion,
        "horaLlegada": datetime.now().isoformat()
    }

    try:
        response = requests.post(API_URL, json=payload, verify=False)
        if response.status_code == 200:
            datos_respuesta = response.json()
            print(f"🚀 Servidor C#: {datos_respuesta.get('mensaje')}")
            print(f"📊 Estado asignado por el motor: {datos_respuesta.get('estadoFinal')} (Retraso real: {datos_respuesta.get('minutosRetrasoReales')} min)")
        else:
            print(f"⚠️ Error del servidor: {response.status_code} - {response.text}")
    except requests.exceptions.ConnectionError:
        print("❌ Error crítico: No se pudo conectar con la API de C#. ¿Está el servidor encendido?")


def detectar_token(id_estudiante=1, id_sesion=1, frecuencia_objetivo=19500, tolerancia=200, duracion_escucha=3):
    fs = 44100
    print(f"🎙️ Activando micrófono... Buscando señal en {frecuencia_objetivo} Hz para la sesión {id_sesion}.")

    grabacion = sd.rec(int(duracion_escucha * fs), samplerate=fs, channels=1, dtype='float32')
    sd.wait()

    audio_data = grabacion[:, 0]

    fft_data = np.fft.fft(audio_data)
    frecuencias = np.fft.fftfreq(len(fft_data), 1 / fs)

    mitad = len(fft_data) // 2
    magnitudes = np.abs(fft_data[:mitad])
    frecuencias_positivas = frecuencias[:mitad]

    filtro_alta_frecuencia = frecuencias_positivas > 15000
    frecuencias_filtradas = frecuencias_positivas[filtro_alta_frecuencia]
    magnitudes_filtradas = magnitudes[filtro_alta_frecuencia]

    if len(magnitudes_filtradas) == 0:
        print("❌ No se detectó ninguna señal de alta frecuencia.")
        return False

    indice_pico = np.argmax(magnitudes_filtradas)
    frecuencia_pico = frecuencias_filtradas[indice_pico]

    print(f"🔍 Frecuencia dominante detectada: {frecuencia_pico:.2f} Hz")

    if abs(frecuencia_pico - frecuencia_objetivo) <= tolerancia:
        print("✅ ¡Token ultrasónico validado localmente por el dispositivo!")
        enviar_asistencia_a_csharp(id_estudiante, id_sesion)
        return True
    else:
        print("❌ Token no detectado. Solo se captó ruido ambiental.")
        return False


if __name__ == "__main__":
    detectar_token(id_estudiante=1, id_sesion=1)
