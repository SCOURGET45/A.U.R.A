import numpy as np
import sounddevice as sd


def detectar_token(frecuencia_objetivo=19500, tolerancia=200, duracion_escucha=3):
    """
    Escucha el entorno y analiza el espectro de frecuencias para detectar el token.
    """
    fs = 44100
    print(f"Activando micrófono... Buscando señal en {frecuencia_objetivo} Hz.")

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

    print(f"Frecuencia dominante detectada: {frecuencia_pico:.2f} Hz")

    if abs(frecuencia_pico - frecuencia_objetivo) <= tolerancia:
        print("✅ ¡ASISTENCIA REGISTRADA EXITOSAMENTE! Token validado.")
        return True
    else:
        print("❌ Token no detectado. Solo se captó ruido de fondo.")
        return False


if __name__ == "__main__":
    detectar_token()
