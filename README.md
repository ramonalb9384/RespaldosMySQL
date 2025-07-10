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
*   **Interfaz de Usuario Nítida**: Soporte para pantallas de alta resolución (High-DPI) para una visualización clara y sin borrosidad.
*   **Control del Servicio**: Instala, desinstala, inicia y detiene el servicio de Windows directamente desde la UI.
*   **Monitoreo en Tiempo Real**: Visualiza el log de actividad del servicio en un `TextBox` en la UI, con actualización automática tipo "tail -f".
*   **Detección y Ejecución de Respaldos Omitidos**: El servicio detecta si un respaldo programado se omitió (por ejemplo, si el servidor estuvo apagado) y lo ejecuta automáticamente al iniciar, respetando una ventana de no respaldo configurable.
*   **Ventana de No Respaldo**: Permite definir un horario durante el cual los respaldos omitidos no se ejecutarán, evitando así la sobrecarga del sistema en horas críticas.
*   **Arquitectura Optimizada**: Lógica de gestión de servidores consolidada en `BackupManager` para una mayor eficiencia y mantenibilidad.

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
*   **MySQL Client**: Con la herramienta `mysqldump` disponible.
*   **7-Zip**: Asegúrate de que el ejecutable `7z.exe` esté especifica su ruta completa en la configuración global de la aplicación.
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
    *   **Importante**: Asegúrate de que las rutas a `mysqldump.exe` y `7z.exe` en la sección `<GlobalSettings>` de `servers.xml` sean correctas para tu sistema. También puedes configurar la `NoBackupWindow` (ventana de no respaldo) en la sección de configuración global de la aplicación para evitar que los respaldos omitidos se ejecuten en horarios específicos.

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

-- Otorga los privilegios necesarios para mysqldump
-- SELECT: Para leer datos de las tablas.
-- LOCK TABLES: Para bloquear tablas durante el respaldo (si no se usa --single-transaction).
-- RELOAD (o SUPER): Necesario para FLUSH TABLES WITH READ LOCK (si se usa --single-transaction o --master-data).
-- SHOW DATABASES: Para listar las bases de datos.
-- PROCESS: Para ver los procesos de MySQL (útil para mysqldump).
-- CREATE TEMPORARY TABLES: Necesario para algunas operaciones internas de mysqldump.
-- EVENT: Si tienes eventos programados en MySQL y quieres que mysqldump los incluya.
GRANT SELECT, LOCK TABLES, RELOAD, SHOW DATABASES, PROCESS, CREATE TEMPORARY TABLES, SHOW VIEW, TRIGGER, EVENT ON *.* TO 'backup_user'@'localhost';

-- Recarga los privilegios
FLUSH PRIVILEGES;
```

**Nota sobre el privilegio `RELOAD` / `SUPER`:**
Si `mysqldump` se ejecuta con la opción `--single-transaction` (recomendado para bases de datos InnoDB para respaldos sin bloqueo) o `--master-data`, puede que necesite el privilegio `RELOAD` o `SUPER` para ejecutar `FLUSH TABLES WITH READ LOCK`. Si encuentras errores de permisos relacionados con `FLUSH TABLES`, asegúrate de que el usuario tenga `RELOAD` o considera otorgar `SUPER` (con precaución, ya que `SUPER` otorga muchos privilegios).
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

## Manejo de Contraseñas y Seguridad

Las contraseñas de los servidores MySQL configurados en la aplicación se almacenan en el archivo `servers.xml` de forma **encriptada** y **nunca se exponen en los logs**.
La encriptación se realiza utilizando `System.Security.Cryptography.ProtectedData` con `DataProtectionScope.LocalMachine`. Esto significa que:

*   Las contraseñas están protegidas contra la lectura directa del archivo.
*   Solo pueden ser desencriptadas en la **misma máquina** donde fueron encriptadas y por el **mismo usuario** (o cualquier usuario si se usa `LocalMachine` y el contexto de la aplicación lo permite) que las encriptó. Esto proporciona una capa de seguridad razonable para la mayoría de los entornos de usuario único.

**Consideraciones de Seguridad Adicionales:**
*   Se implementa una **validación estricta de las entradas** en la interfaz de usuario y en la lógica de respaldo para prevenir ataques de inyección de comandos, asegurando que los datos de configuración sean seguros antes de ser utilizados en operaciones críticas.
*   Aunque las contraseñas están encriptadas, el archivo `servers.xml` sigue siendo un recurso sensible. Asegúrate de que los permisos del sistema de archivos restrinjan el acceso a usuarios no autorizados.
*   Si necesitas mover la configuración a otra máquina o a otro usuario, las contraseñas no podrán ser desencriptadas automáticamente. En ese caso, deberás reintroducirlas manualmente en la aplicación.

## Exportar e Importar Configuración

La aplicación permite exportar e importar la configuración de los servidores (incluyendo las contraseñas encriptadas) a un archivo `.encfg` (encrypted config). Esta funcionalidad es útil para:

*   **Migrar configuraciones** entre diferentes instalaciones o usuarios (aunque las contraseñas deberán reintroducirse si se mueven a una máquina diferente).
*   **Realizar copias de seguridad** de tu configuración.
*   **Compartir configuraciones** de forma segura (siempre y cuando la contraseña de encriptación del archivo `.encfg` sea compartida de forma segura).

**Proceso de Exportación:**
1.  Haz clic en `Herramientas > Exportar/Importar > Exportar Configuración`.
2.  Se te pedirá una contraseña para encriptar el archivo `.encfg`. Esta contraseña es **diferente** de las contraseñas de MySQL y se utiliza para proteger el archivo de exportación.
3.  Guarda el archivo `.encfg` en la ubicación deseada.

**Proceso de Importación:**
1.  Haz clic en `Herramientas > Exportar/Importar > Importar Configuración`.
2.  Selecciona el archivo `.encfg` que deseas importar.
3.  Se te pedirá la contraseña que utilizaste para encriptar el archivo durante la exportación.
4.  Una vez desencriptado, la configuración se cargará en la aplicación, sobrescribiendo la configuración actual. Las contraseñas de MySQL se desencriptarán si la importación se realiza en la misma máquina y por el mismo usuario; de lo contrario, deberás reintroducirlas.

**Nota:** La importación de configuración requiere privilegios de administrador para asegurar que la aplicación pueda escribir en la ubicación del archivo `servers.xml` y manejar la encriptación/desencriptación de forma segura.

## Contribuciones

Las contribuciones son bienvenidas. Por favor, abre un "issue" para discutir los cambios propuestos o envía un "pull request".

## Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo `LICENSE` para más detalles.