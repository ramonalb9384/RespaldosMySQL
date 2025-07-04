# RespaldosMysql

## Descripción del Proyecto

`RespaldosMysql` es una solución integral para la gestión y automatización de respaldos de bases de datos MySQL. El proyecto consta de tres componentes principales:

1.  **`RespaldosMysqlLibrary`**: Una biblioteca de clases que contiene la lógica central para la conexión a MySQL, la ejecución de `mysqldump` y la compresión de los respaldos.
2.  **`RespaldosMysqlService`**: Un servicio de Windows que se ejecuta en segundo plano, encargado de realizar los respaldos programados de forma automática según la configuración definida.
3.  **`RespaldosMysqlUI`**: Una aplicación de interfaz de usuario (Windows Forms) que permite a los usuarios configurar los servidores MySQL, gestionar los horarios de respaldo, realizar respaldos manuales, instalar/desinstalar/iniciar/detener el servicio de Windows, y monitorear el log de actividad en tiempo real.

## Características

*   **Respaldo Automatizado**: El servicio de Windows realiza respaldos de MySQL de forma programada.
*   **Configuración Flexible**: Permite añadir múltiples servidores MySQL con configuraciones individuales.
*   **Programación Detallada**: Define horarios específicos y días de la semana para cada respaldo.
*   **Exclusión de Bases de Datos**: Posibilidad de excluir bases de datos específicas del proceso de respaldo.
*   **Compresión de Respaldos**: Los archivos de respaldo SQL se comprimen automáticamente utilizando 7-Zip para ahorrar espacio.
*   **Interfaz de Usuario Intuitiva**: Gestión sencilla de servidores y configuración a través de una aplicación de escritorio.
*   **Control del Servicio**: Instala, desinstala, inicia y detiene el servicio de Windows directamente desde la UI.
*   **Monitoreo en Tiempo Real**: Visualiza el log de actividad del servicio en un `TextBox` en la UI, con actualización automática tipo "tail -f".
*   **Manejo Seguro de Credenciales**: Las cadenas de conexión se construyen de forma segura utilizando `MySqlConnectionStringBuilder` para manejar correctamente caracteres especiales en las contraseñas.

## Tecnologías Utilizadas

*   **Visual Basic .NET**: Lenguaje de programación principal.
*   **.NET Framework**: Plataforma de desarrollo.
*   **MySQL.Data**: Conector ADO.NET para MySQL.
*   **mysqldump**: Herramienta de línea de comandos de MySQL para realizar los respaldos.
*   **7-Zip**: Herramienta de compresión de archivos (se requiere el ejecutable `7z.exe`).
*   **Windows Services**: Para la automatización en segundo plano.
*   **Windows Forms**: Para la interfaz de usuario.

## Requisitos Previos

Antes de compilar y ejecutar la aplicación, asegúrate de tener instalado lo siguiente:

*   **Visual Studio**: Para abrir y compilar la solución.
*   **.NET Framework**: La versión compatible con el proyecto (generalmente 4.8 o superior para proyectos de escritorio modernos).
*   **MySQL CLient**: Con la herramienta `mysqldump` disponible. Asegúrate de que `mysqldump.exe` esté especificada su ruta completa en la configuración global de la aplicación.
*   **7-Zip**: Asegúrate de que el ejecutable `7z.exe` esté especificada su ruta completa en la configuración global de la aplicación.
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
    *   **Añadir Servidor**: Haz clic en "Añadir" para configurar un nuevo servidor MySQL (IP, Puerto, Usuario, Contraseña).
    *   **Editar Servidor**: Selecciona un servidor de la lista y haz clic en "Editar" para modificar su configuración.
    *   **Eliminar Servidor**: Selecciona un servidor y haz clic en "Eliminar" para quitarlo de la lista.
*   **Configuración de Respaldo**:
    *   En el editor de servidores, puedes seleccionar las bases de datos a excluir del respaldo.
    *   Configura el horario de respaldo (habilitar, días de la semana, hora).
*   **Respaldo Manual**:
    *   Selecciona un servidor y haz clic en "Respaldar Ahora" para iniciar un respaldo manual inmediato. Se te pedirá que elijas la carpeta de destino.
*   **Configuración Global**:
    *   Haz clic en "Configuración" para ajustar las rutas globales de `mysqldump.exe`, `7z.exe` y la carpeta de destino de los respaldos.
*   **Control del Servicio**:
    *   Los botones "Instalar Servicio", "Desinstalar Servicio", "Iniciar Servicio" y "Detener Servicio" te permiten gestionar el ciclo de vida del servicio del Windows.
    *   La etiqueta "Estado del Servicio" mostrará el estado actual del servicio (Corriendo, Detenido, No Instalado, etc.) y se actualizará automáticamente.
*   **Log de Actividad**:
    *   El `TextBox` en la parte inferior de la UI muestra el log de actividad del servicio en tiempo real, como un "tail -f".

### Servicio de Windows (`RespaldosMysqlService.exe`)

Una vez instalado e iniciado, el servicio se ejecutará en segundo plano y realizará los respaldos programados automáticamente. Puedes monitorear su actividad a través del log en la aplicación UI o en el Visor de Eventos de Windows.

## Permisos de Usuario MySQL

El usuario MySQL configurado para los respaldos debe tener los siguientes privilegios mínimos:

```sql
-- Crea el usuario (si no existe)
CREATE USER 'backup_user'@'localhost' IDENTIFIED BY 'your_password';

-- Otorga los privilegios necesarios
GRANT SELECT, LOCK TABLES, RELOAD, SHOW DATABASES, PROCESS ON *.* TO 'backup_user'@'localhost';

-- Recarga los privilegios
FLUSH PRIVILEGES;
```
*   Reemplaza `'backup_user'` y `'your_password'` con tus credenciales.
*   Ajusta `'localhost'` a la IP o nombre de host desde donde se conectará tu aplicación/servicio. Usa `'%'` si la conexión puede venir de cualquier host (menos seguro).

## Solución de Problemas Comunes

*   **Servicio no se inicia**:
    *   Verifica el log en la UI o en el Visor de Eventos de Windows para mensajes de error.
    *   Asegúrate de que las rutas a `mysqldump.exe` y `7z.exe` en `servers.xml` sean correctas.
    *   Verifica que el usuario MySQL tenga los permisos adecuados.
*   **`servers.xml` no se guarda/carga correctamente**:
    *   Asegúrate de que la aplicación UI y el servicio tengan permisos de escritura en la carpeta donde se encuentra `servers.xml` (generalmente la carpeta `bin\Debug` o `bin\Release`).
*   **Errores de conexión a MySQL**:
    *   Verifica la IP, puerto, usuario y contraseña en la configuración del servidor.
    *   Asegúrate de que el servidor MySQL esté en ejecución y sea accesible desde la máquina donde se ejecuta el servicio/UI.
    *   Confirma que el usuario MySQL tiene los permisos correctos.
*   **`mysqldump` o `7z.exe` no encontrados**:
    *   Verifica las rutas configuradas en la sección de configuración global de la aplicación.
    *   Asegúrate de que los ejecutables existan en esas rutas.

## Contribuciones

Las contribuciones son bienvenidas. Por favor, abre un "issue" para discutir los cambios propuestos o envía un "pull request".

## Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo `LICENSE` para más detalles.
