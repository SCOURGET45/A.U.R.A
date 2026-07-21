import numpy as np
import scipy.io.wavfile as wav


def generar_token_ultrasonico(frecuencia=19500, duracion_total=3.0, id_aula="101", archivo_salida="token_aula.wav"):
    tasa_muestreo = 44100
    t = np.linspace(0, duracion_total, int(tasa_muestreo * duracion_total), endpoint=False)

    onda_portadora = np.sin(2 * np.pi * frecuencia * t)

    modulador = np.zeros_like(t)

    modulador[(t > 0.5) & (t < 1.0)] = 1.0
    modulador[(t > 1.5) & (t < 2.0)] = 1.0
    modulador[(t > 2.5) & (t < 3.0)] = 1.0

    senal_final = onda_portadora * modulador
    senal_final = np.int16(senal_final * 32767)

    wav.write(archivo_salida, tasa_muestreo, senal_final)
    print(f"Token de {frecuencia} Hz generado con éxito: {archivo_salida}")


if __name__ == "__main__":
    generar_token_ultrasonico()
