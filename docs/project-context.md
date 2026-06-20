# Project Context

Este documento resume el estado arquitectónico actual del proyecto y sirve como punto de partida para nuevos desarrolladores o nuevas sesiones de trabajo.

## Tecnologías

* Lenguaje: C#
* Framework: .NET 10
* IDE principal: Visual Studio Code
* Plataforma principal de desarrollo: Linux
* Framework de pruebas: xUnit
* Assertions: FluentAssertions

## Solución

```text
IronShield.sln

src/
├── IronShield.Core
├── IronShield.Cryptography
├── IronShield.Storage
└── IronShield.Cli

tests/
├── IronShield.Core.Tests
├── IronShield.Cryptography.Tests
└── IronShield.Storage.Tests
```

## Principales decisiones ya tomadas

* Formato de archivo oficial: `.iron`.
* Contenedor basado en bloques independientes.
* AES-256-GCM para cifrado.
* Argon2id para derivación de claves.
* SHA-256 para integridad inicial.
* Serialización JSON para los datos internos de los bloques.
* El proyecto controla explícitamente la serialización polimórfica.
* No se utilizan discriminadores automáticos como `$type`.

## Estado del modelo de dominio

### IronContainer

Contenedor principal del archivo `.iron`. Incluye:

* Versión del formato.
* Colección de bloques.

### IronBlock

Representa una unidad autocontenida de información.

Propiedades:

* Tipo de bloque.
* Indicador de si el bloque está cifrado.
* Datos serializados.

## Capa de obtención de datos

Se introdujo una capa abstracta para obtener datos desde distintos orígenes.

### Interfaces (Core)

* `IDataSource` — representa cualquier origen de datos (nombre, tamaño, stream de lectura).
* `IIronBlockDataFactory` — factoría que construye `IIronBlockData[]` (metadata, contenido e integridad) desde un `IDataSource`.
* `IIronProtectionService` — servicio de protección (datos → `.iron` cifrado).
* `IIronUnprotectionService` — servicio de desprotección (`.iron` → datos originales).
* `IIronShieldService` — fachada que unifica Protection + Unprotection.

### Implementaciones (Storage.Sources)

* `FileDataSource` — lee un archivo del sistema de archivos.
* `DirectoryDataSource` — comprime el directorio completo a un ZIP en memoria. El ZIP viaja como un solo `FileContent` (Caso A: ZIP embebido). Ver sección "Evolución futura" para el Caso B (bloques estructurados).
* `CompressedDataSource` — decorador que envuelve cualquier `IDataSource` y aplica GZip.
* `IronBlockDataFactory` — implementa `IIronBlockDataFactory`; lee el `IDataSource`, calcula hash (opcional), construye `PublicMetadata`, `FileContent` e `IntegrityData`.

### Implementaciones (Storage.Services)

* `IronProtectionService` — implementa `IIronProtectionService` (4 dependencias: `IIronBlockDataFactory`, `IIronCryptographyContextFactory`, `IIronContainerFactory`, `IIronContainerWriter`).
* `IronUnprotectionService` — implementa `IIronUnprotectionService` (4 dependencias: `IIronContainerReader`, `IIronBlockSerializer`, `IEncryptionProvider`, `IKeyDerivationProvider`).
* `IronShieldService` — fachada que implementa `IIronShieldService` (2 dependencias: `IIronProtectionService` + `IIronUnprotectionService`). También expone un constructor de conveniencia con los 5 primitivos (`IHashProvider`, `IEncryptionProvider`, `IKeyDerivationProvider`, `IIronBlockSerializer`, `IIronEncryptionProfile`).

### Modelos nuevos (Core.Models)

* `UnprotectResult` — resultado de Unprotect: `Data` (bytes originales) + `Metadata` opcional.

### Evolución futura: directorios estructurados (Caso B)

Actualmente `DirectoryDataSource` empaqueta el directorio como un ZIP en un solo `FileContent`. A futuro puede migrarse a bloques individuales por archivo sin romper compatibilidad:

| Paso | Cambio | Impacto |
|---|---|---|
| 1 | Agregar `IronBlockType.DirectoryEntry = 5` | Nuevo valor enum, no rompe existentes |
| 2 | Agregar `DirectoryEntry : IIronBlockData` con `RelativePath`, `Content`, `LastWriteUtc` | Nuevo modelo, no afecta otros |
| 3 | Modificar `IronBlockDataFactory.Create()` para directorios: emitir N `DirectoryEntry` en vez de ZIP | Solo cambia la factoría |
| 4 | `IronUnprotectionService` lee y reconstruye estructura | Nuevo switch case |

Los `.iron` existentes (con ZIP en `FileContent`) seguirán siendo legibles. Los archivos nuevos con `DirectoryEntry` serán ignorados por lectores antiguos (forward compatibility por diseño).

### Flujo completo

```text
== Protect ==

IDataSource (FileDataSource / DirectoryDataSource / CompressedDataSource)
    ↓
IDataCollector.Collect(source)
    ↓
IIronBlockData[]  →  IronCryptographyContextFactory.Create(password)
                    ↓
                IronContainerFactory.Create(version, blocks, cryptoContext)
                    ↓
                IronContainer  →  IronContainerWriter.Write(container, stream)
                    ↓
                Archivo .iron

== Unprotect ==

Archivo .iron
    ↓
IronContainerReader.Read(stream)
    ↓
IronContainer  →  extraer EncryptionInfo → derivar clave
    ↓
para cada bloque: descifrar si es necesario → deserializar
    ↓
UnprotectResult { Data, Metadata }
```

## Próximo objetivo técnico

Desarrollar la CLI (comandos `create`, `extract`, `info`, `verify`) utilizando el orquestador `IronShieldService`.

Este documento debe mantenerse actualizado después de cada decisión arquitectónica importante.