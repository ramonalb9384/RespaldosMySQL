# RespaldosMysql

## Descripción del Proyecto

`RespaldosMysql` es una solución integral para la gestión y automatización de respaldos de bases de datos MySQL. El proyecto consta de tres componentes principales:

1.  **`RespaldosMysqlLibrary`**: Una biblioteca de clases que contiene la lógica central para la conexión a MySQL, la ejecución de `mysqldump` y la compresión de los respaldos.
2.  **`RespaldosMysqlService`**: Un servicio de Windows que se ejecuta en segundo plano, encargado de realizar los respaldos programados de forma automática según la configuración definida.
3.  **`RespaldosMysqlUI`**: Una aplicación de interfaz de usuario (Windows Forms) que permite a los usuarios configurar los servidores MySQL, gestionar los horarios de respaldo, realizar respaldos manuales, instalar/desinstalar/iniciar/detener el servicio de Windows, y monitorear el log de actividad en tiempo real.

## Características

*   **Respaldo Automatizado**: El servicio de Windows realiza respaldos de MySQL de forma programada.
*   **Parámetros Adicionales para mysqldump**: Permite agregar parámetros personalizados al comando `mysqldump` para mayor flexibilidad en los respaldos.
*   **Respaldo Manual Asíncrono**: La interfaz de usuario ya no se congela durante los respaldos manuales.
*   **Respaldos por Evento**: Permite programar respaldos únicos para fechas y horas específicas, ideales para ventanas de mantenimiento o eventos puntuales.
*   **Configuración Flexible**: Permite añadir múltiples servidores MySQL con configuraciones individuales.
*   **Programación Detallada**: Define horarios específicos y días de la semana para cada respaldo.
*   **Exclusión de Bases de Datos**: Posibilidad de excluir bases de datos específicas del proceso de respaldo.
*   **Compresión de Respaldos**: Los archivos de respaldo SQL y ZIP se comprimen automáticamente utilizando 7-Zip para ahorrar espacio. Los archivos ZIP incluyen la fecha y hora (`YYMMDD_HHMM`) en su nombre para una mejor identificación.
*   **Interfaz de Usuario Intuitiva**: Gestión sencilla de servidores y configuración a través de una aplicación de escritorio.
*   **Interfaz de Usuario Nítida**: Soporte para pantallas de alta resolución (High-DPI) para una visualización clara y sin borrosidad.
*   **Control del Servicio**: Instala, desinstala, inicia y detiene el servicio de Windows directamente desde la UI.
*   **Monitoreo en Tiempo Real**: Visualiza el log de actividad del servicio en un `TextBox` en la UI, con actualización automática tipo "tail -f".
*   **Notificaciones Push**: Envía notificaciones al inicio y al final de cada respaldo a tu celular a través del servicio [ntfy.sh](https://ntfy.sh), permitiéndote usar el servicio público o tu propio servidor autoalojado.
*   **Detección y Ejecución de Respaldos Omitidos**: El servicio detecta si un respaldo programado se omitió (por ejemplo, si el servidor estuvo apagado) y lo ejecuta automáticamente al iniciar, respetando una ventana de no respaldo configurable.
*   **Ventana de No Respaldo**: Permite definir un horario durante el cual los respaldos omitidos no se ejecutarán, evitando así la sobrecarga del sistema en horas críticas.
*   **Arquitectura Optimizada**: Lógica de gestión de servidores consolidada en `BackupManager` para una mayor eficiencia y mantenibilidad.

## Tecnologías Utilizadas

*   **Visual Basic .NET**: Lenguaje de programación principal.
*   **.NET Framework**: Plataforma de desarrollo.
*   **MySQL.Data**: Conector ADO.NET para MySQL.
*   **mysqldump**: Herramienta de línea de comandos de MySQL para realizar los respaldos.
*   **7-Zip**: Herramienta de compresión de archivos (se requiere el ejecutable `7z.exe`).
*   **ntfy.sh**: Servicio de notificaciones push.
*   **Windows Services**: Para la automatización en segundo plano.
*   **Windows Forms**: Para la interfaz de usuario.

## Requisitos Previos

Antes de compilar y ejecutar la aplicación, asegúrate de tener instalado lo siguiente:

*   **Visual Studio**: Para abrir y compilar la solución.
*   **.NET Framework**: La versión compatible con el proyecto (generalmente 4.8 o superior para proyectos de escritorio modernos).
*   **MySQL Client**: Con la herramienta `mysqldump` disponible.
*   **7-Zip**: Asegúrate de que el ejecutable `7z.exe` esté especifica su ruta completa en la configuración global de la aplicación.
*   **App ntfy.sh**: (Opcional) Si deseas recibir notificaciones, instala la app en tu celular.
*   **Privilegios de Administrador**: Para instalar, desinstalar, iniciar o detener el servicio de Windows, la aplicación `RespaldosMysqlUI` debe ejecutarse con privilegios de administrador.

## Configuración e Instalación

1.  **Clonar el Repositorio**:
    ```bash
    git clone https://github.com/ramonalb9384/RespaldosMySQL
    cd RespaldosMysql
    ```
 
2.  **Abrir en Visual Studio**:
    Abre el archivo de solución `RespaldosMysql.sln` en Visual Studio.

3.  **Compilar la Solución**:
    Compila toda la solución (`Ctrl+Shift+B` o `Build > Build Solution`). Esto generará los ejecutables en las carpetas `bin\Debug` o `bin\Release` de cada proyecto.

4.  **Configurar `servers.xml`**:
    El archivo de configuración `servers.xml` se encuentra en la misma carpeta que el ejecutable de la UI (`RespaldosMysqlUI\bin\Debug\servers.xml`) y el servicio (`RespaldosMysqlService\bin\Debug\servers.xml`).
    *   Si el archivo no existe, la aplicación creará uno por defecto con un servidor de ejemplo.
    *   Puedes editar este archivo manualmente o, preferiblemente, usar la interfaz de usuario para gestionar los servidores.
    *   **Importante**: Asegúrate de que las rutas a `mysqldump.exe` y `7z.exe` en la sección `<GlobalSettings>` de `servers.xml` sean correctas para tu sistema.

5.  **Instalar el Servicio de Windows**:
    *   Ejecuta `RespaldosMysqlUI.exe` **como administrador**.
    *   Utiliza el botón "Instalar Servicio" en la interfaz de usuario. Esto registrará el servicio `RespaldosMysqlService` en tu sistema.

## Uso

### Interfaz de Usuario (`RespaldosMysqlUI.exe`)

*   **Gestión de Servidores**:
    *   **Añadir/Editar Servidor**: Configura los servidores MySQL, los horarios de respaldo y las notificaciones.
    *   **Eliminar Servidor**: Selecciona un servidor y haz clic en "Eliminar" para quitarlo de la lista.
*   **Respaldo Manual**:
    *   Selecciona un servidor y haz clic en "Respaldar Ahora" para iniciar un respaldo manual inmediato.
*   **Configuración Global**:
    *   Haz clic en "Configuración" para ajustar las rutas globales de `mysqldump.exe`, `7z.exe` y la carpeta de destino de los respaldos.
*   **Control del Servicio**:
    *   Gestiona el ciclo de vida del servicio de Windows (Instalar, Desinstalar, Iniciar, Detener).
*   **Log de Actividad**:
    *   Monitorea el log del servicio en tiempo real.

## Notificaciones con ntfy.sh

La aplicación se integra con el servicio de notificaciones push de código abierto [ntfy.sh](https://ntfy.sh) para mantenerte informado sobre el estado de tus respaldos directamente en tu celular.

### ¿Cómo funciona?

1.  **Instala ntfy**: Descarga la aplicación de ntfy para [Android](https://play.google.com/store/apps/details?id=io.heckel.ntfy) o [iOS](https://apps.apple.com/us/app/ntfy/id1625396347).
2.  **Suscríbete a un tema**: En la aplicación móvil, suscríbete a un "tema" (topic). Este es un nombre secreto que solo tú conocerás (ej: `respaldos-de-mi-empresa-123`).
3.  **Configura en la UI**:
    *   En `RespaldosMysqlUI`, edita un servidor.
    *   Marca la casilla "Habilitar notificaciones para este servidor".
    *   En el campo "URL del Tema", introduce la URL completa de tu tema.
        *   Para el servicio público: `ntfy.sh/nombre-de-tu-tema-secreto`
        *   Para un servidor propio: `https://ntfy.tu-dominio.com/nombre-de-tu-tema-secreto`
4.  **Recibe Notificaciones**: ¡Listo! Ahora recibirás una notificación cuando un respaldo inicie, finalice con éxito o falle.

Las notificaciones incluyen:
*   **Inicio del respaldo**: Con un icono de reloj de arena.
*   **Respaldo exitoso**: Con un icono de marca de verificación.
*   **Fallo en el respaldo**: Con un icono de 'X' y prioridad alta para alertarte del problema.

## Manejo de Contraseñas y Seguridad

Las contraseñas de los servidores MySQL configurados en la aplicación se almacenan en el archivo `servers.xml` de forma **encriptada** y **nunca se exponen en los logs**.
La encriptación se realiza utilizando `System.Security.Cryptography.ProtectedData` con `DataProtectionScope.LocalMachine`. Esto significa que:

*   Las contraseñas están protegidas contra la lectura directa del archivo.
*   Solo pueden ser desencriptadas en la **misma máquina** donde fueron encriptadas.

**Consideraciones de Seguridad Adicionales:**
*   Se implementa una **validación estricta de las entradas** para prevenir ataques de inyección de comandos.
*   Asegúrate de que los permisos del sistema de archivos restrinjan el acceso al archivo `servers.xml`.

## Contribuciones

Las contribuciones son bienvenidas. Por favor, abre un "issue" para discutir los cambios propuestos o envía un "pull request".

## Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo `LICENSE` para más detalles.
