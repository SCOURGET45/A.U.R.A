import pyaudio
import numpy as np
import time

CHUNK = 2048
FORMAT = pyaudio.paInt16
CHANNELS = 1
RATE = 44100
TARGET_FREQ = 19500
TOLERANCIA = 200


def detectar_token():
    p = pyaudio.PyAudio()
    stream = p.open(format=FORMAT, channels=CHANNELS, rate=RATE, input=True, frames_per_buffer=CHUNK)

    print(f"Escuchando frecuencias en el rango de {TARGET_FREQ} Hz...")

    try:
        while True:
            data = stream.read(CHUNK, exception_on_overflow=False)
            muestras = np.frombuffer(data, dtype=np.int16)

            fft_resultado = np.fft.rfft(muestras)
            frecuencias = np.fft.rfftfreq(CHUNK, d=1.0 / RATE)
            magnitudes = np.abs(fft_resultado)

            indice_pico = np.argmax(magnitudes)
            frecuencia_pico = frecuencias[indice_pico]
            energia_pico = magnitudes[indice_pico]

            if energia_pico > 100000:
                if (TARGET_FREQ - TOLERANCIA) < frecuencia_pico < (TARGET_FREQ + TOLERANCIA):
                    print(f"[{time.strftime('%H:%M:%S')}] ¡VALIDACIÓN EXITOSA! Token de {frecuencia_pico:.0f} Hz detectado.")

    except KeyboardInterrupt:
        print("Deteniendo escucha...")
    finally:
        stream.stop_stream()
        stream.close()
        p.terminate()


if __name__ == "__main__":
    detectar_token()
