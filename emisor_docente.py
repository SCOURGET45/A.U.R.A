import numpy as np
import sounddevice as sd


def emitir_token(frecuencia=19500, duracion=3.0, volumen=0.5):
    """
    Genera y reproduce un tono inaudible de 19.5 kHz.
    """
    print(f"Emisión de token a {frecuencia} Hz iniciada...")

    fs = 44100
    t = np.linspace(0, duracion, int(fs * duracion), False)
    onda = volumen * np.sin(2 * np.pi * frecuencia * t)

    sd.play(onda, fs)
    sd.wait()

    print("Emisión del token ultrasónico finalizada.")


if __name__ == "__main__":
    emitir_token()
