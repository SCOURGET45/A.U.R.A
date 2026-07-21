
# A.U.R.A. 🎙️📊
**Asistencia Ultrasónica y Registro Académico (Proyecto SCSTUDY)**

A.U.R.A. es un ecosistema de software y hardware diseñado para erradicar el desgaste administrativo del pase de lista en instituciones educativas. Utiliza tecnología de transmisión acústica inaudible (19.5 kHz) para registrar la asistencia con "fricción cero", y un motor de reglas de negocio en el backend para predecir y mitigar el riesgo de deserción estudiantil.

---

## 🚀 Características Principales

El sistema está segregado por roles, garantizando la seguridad y el flujo correcto de los procesos académicos:

*   **Pase de Lista Ultrasónico (Fricción Cero):** Los docentes emiten un token ultrasónico de 19.5 kHz. Los dispositivos de los alumnos utilizan la Transformada Rápida de Fourier (FFT) para validar la frecuencia en tiempo real y registrar la asistencia automáticamente.
*   **Motor de Reglas Automático:** Convierte automáticamente retardos en faltas (3 retardos = 1 falta) sin intervención humana.
*   **Workflow de Vulnerabilidad:** Un sistema de escalamiento donde los Tutores solicitan excepciones (por salud, transporte, etc.) y la Dirección aprueba márgenes de tolerancia dinámicos.
*   **Carga Masiva (Secretaría):** Poblamiento rápido de la base de datos mediante archivos CSV optimizados en memoria.
*   **Dashboards Preventivos:** Semáforos de unidad y termómetros de asistencia global para que el estudiante conozca su estatus al instante.

## 🛠️ Stack Tecnológico

**Backend & Web**
*   **Lenguaje / Framework:** C# con ASP.NET Core MVC
*   **ORM:** Entity Framework Core
*   **Base de Datos:** SQL Server
*   **Frontend:** HTML5, CSS3, Bootstrap 5, Razor Pages

**Hardware & Procesamiento de Señales (Cliente)**
*   **Lenguaje:** Python 3
*   **Librerías Clave:** `numpy` (Matemáticas), `sounddevice` (Captura de audio), `requests` (Comunicación HTTP)
*   **Técnica:** Transformada Rápida de Fourier (FFT) para aislamiento de espectro.

---

## ⚙️ Instalación y Despliegue Local

### 1. Clonar el repositorio
```bash
git clone [https://github.com/tu-usuario/scstudy-aura.git](https://github.com/tu-usuario/scstudy-aura.git)
cd scstudy-aura








1. Resumen Ejecutivo y Alcance del Proyecto
1.1. Planteamiento del Problema: El desgaste administrativo del pase de lista tradicional y el riesgo de deserción por falta de seguimiento.

1.2. Solución Propuesta (A.U.R.A.): Presentación del ecosistema de Asistencia Ultrasónica y Registro Académico con enfoque de "fricción cero".

1.3. Alcance: Delimitación del proyecto (pase de lista ultrasónico, motor de inasistencias y workflow de justificaciones).

2. Arquitectura del Sistema
2.1. Patrón Arquitectónico (MVC): Justificación del uso del Modelo-Vista-Controlador para separar los datos, la lógica de negocio y las interfaces de usuario.

2.2. Diagrama de Componentes: Explicación de cómo interactúa el Servidor Web (C#), la Base de Datos (SQL) y los Dispositivos Cliente (Micrófonos/Python).

2.3. Pila Tecnológica (Stack):

Backend: ASP.NET Core (C#), Entity Framework Core.

Frontend: Razor Views, HTML5, Bootstrap 5.

Hardware/Señales: Python, numpy, sounddevice (Procesamiento de señales).

3. Integración de Hardware: El Token Ultrasónico
3.1. Generación de Señal: Cómo el dispositivo del docente emite un tono inaudible de 19.5 kHz.

3.2. Detección y Procesamiento (FFT): Explicación técnica de cómo el script de Python captura el audio ambiental y utiliza la Transformada Rápida de Fourier (FFT) para aislar la frecuencia y evadir el ruido blanco.

3.3. Comunicación API: El disparo de la petición HTTP POST desde el cliente hacia el controlador en C# tras la validación física.

4. Motor de Reglas de Negocio (Core Lógico)
4.1. Algoritmo de Asistencia: Conversión automática (3 retardos = 1 falta).

4.2. Cálculo de Riesgo Académico: Fórmula matemática para determinar si el alumno supera el 20% de inasistencias en base a las clases configuradas.

4.3. Validación de Justificantes: Reglas duras programadas en el backend (límite máximo de 2 por cuatrimestre, tope acumulado de 15 días).

5. Segregación por Roles y Gobernanza
5.1. Módulo del Estudiante: Termómetro global, Semáforo de unidad y transparencia de datos.

5.2. Módulo del Docente: Gestión de sesión "Mi Día", tarjetas dinámicas y descargas bajo demanda.

5.3. Workflow de Vulnerabilidad (Tutor y Director):

Rol del Tutor (Evaluación humana e inicio del trámite).

Rol del Director (Dictamen ejecutivo y activación de la "Tolerancia Automática").

5.4. Módulo de Secretaría: Carga masiva de datos en formato CSV e inicialización del cuatrimestre.

6. Seguridad y Protección de Datos
6.1. Autenticación y Autorización: Uso de Claims y Cookies en ASP.NET Core para bloquear el acceso a URLs no permitidas según el rol [Authorize(Roles = "...")].

6.2. Prevención de Inyecciones: Validación de datos en la subida de archivos (CSV) y en los formularios de vulnerabilidad.

7. Conclusiones y Trabajo Futuro
7.1. Impacto Académico: Cómo A.U.R.A. elimina el tiempo muerto en el aula y previene la deserción.

7.2. Siguientes Pasos (Escalabilidad): Posible migración a la nube o integración de la app de Python a dispositivos móviles nativos (Android/iOS).
