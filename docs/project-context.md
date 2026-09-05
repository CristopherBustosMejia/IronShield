# Project Context

Este documento resume el estado arquitectónico actual del proyecto y sirve como punto de partida para nuevos desarrolladores o nuevas sesiones de trabajo.

## Tecnologías

* Lenguaje: C# (preview LangVersion)
* Framework: .NET 10
* Plataforma: Linux / macOS / Windows
* Framework de pruebas: xUnit + FluentAssertions
* CLI: System.CommandLine 2.0.9 + Spectre.Console 0.49.1

## Solución

```text
IronShield.slnx

src/
├── IronShield.Core            # Interfaces, modelos, perfiles, enumeraciones
├── IronShield.Cryptography    # Proveedores reales (SHA-256, AES-GCM, Argon2id)
├── IronShield.Storage         # Implementación: servicios, serialización, fuentes
└── IronShield.Cli             # CLI (System.CommandLine + Spectre.Console)

tests/
├── IronShield.Core.Tests          # Placeholder (0 tests)
├── IronShield.Cryptography.Tests  # 18 tests
├── IronShield.Storage.Tests       # 69 tests
└── IronShield.Cli.Tests           # 8 tests (unit + integración)
```

## Principales decisiones ya tomadas

* Formato de archivo oficial: `.iron`.
* Contenedor basado en bloques independientes.
* AES-256-GCM para cifrado.
* Argon2id para derivación de claves.
* SHA-256 para integridad inicial.
* Serialización binaria para los datos internos de los bloques.
* El proyecto controla explícitamente la serialización polimórfica (sin `$type`).
* `IDataSource` como abstracción de entrada.
* `IIronBlockDataFactory` (antes `IDataCollector`) construye los bloques desde un `IDataSource`.
* CLI delega toda la lógica a `IIronShieldService` (sin lógica de negocio en CLI).
* `DirectoryDataSource` usa ZIP embebido (Caso A); migración futura a bloques estructurados (Caso B) sin breaking changes.

## Estado del modelo de dominio

### IronContainer

Contenedor principal del archivo `.iron`. Incluye:
* Versión del formato.
* Colección de bloques (`IronBlock[]`).

### IronBlock

Unidad autocontenida de información con:
* Tipo de bloque (`IronBlockType`).
* Indicador de cifrado.
* Datos serializados en binario.

### Bloques actuales

| Tipo | ID | Descripción |
|---|---|---|
| `EncryptionInfo` | 1 | Parámetros de cifrado (algoritmo, nonce, etc.) |
| `PublicMetadata` | 2 | Metadatos visibles (creador, timestamp, nombre original) |
| `FileContent` | 3 | Contenido cifrado del archivo (o ZIP en directorios) |
| `IntegrityData` | 4 | Hash del contenido para verificación |

Los lectores deben **saltar bloques con tipo desconocido** (forward compatibility).

## Capa de servicios

### Interfaces (Core)

* `IDataSource` — origen de datos (nombre, tamaño, stream de lectura).
* `IIronBlockDataFactory` — construye `IIronBlockData[]` desde un `IDataSource`.
* `IHashProvider` — cómputo de hash.
* `IEncryptionProvider` — cifrado/descifrado.
* `IKeyDerivationProvider` — derivación de clave desde password.
* `IIronBlockSerializer` — serialización binaria de bloques.
* `IIronEncryptionProfile` — parámetros de cifrado (KDF, tamaño de clave, etc.).
* `IIronProtectionService` — pipeline protect.
* `IIronUnprotectionService` — pipeline unprotect.
* `IIronShieldService` — fachada unificada.

### Implementaciones (Storage)

* `FileDataSource` — lee un archivo.
* `DirectoryDataSource` — comprime directorio a ZIP en memoria.
* `CompressedDataSource` — decorador GZip sobre cualquier `IDataSource`.
* `IronBlockDataFactory` — construye los bloques desde un `IDataSource`.
* `IronProtectionService` / `IronUnprotectionService` — pipelines.
* `IronShieldService` — fachada con constructor de conveniencia (5 primitivos).

### CLI

* `ProtectCommand` — `protect <path> [-o] [-p] [--creator] [--overwrite]`.
* `UnprotectCommand` — `unprotect <path> [-o] [-p] [--overwrite]`.
* `CliOutputService` — helpers de UX (spinner, colores, errores, password oculta).
* `DependencyInjection` — composition root que construye `IronShieldService`.
* `Program.cs` — mínimo: header → parse → invoke.

## Flujo completo

```text
== Protect ==

IDataSource (FileDataSource / DirectoryDataSource)
    ↓
IIronBlockDataFactory.Create(source)
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

## CLI commands

```bash
# Proteger un archivo
dotnet run --project src/IronShield.Cli -- protect documento.pdf -p "clave" -o documento.pdf.iron

# Proteger un directorio
dotnet run --project src/IronShield.Cli -- protect ./carpeta -p "clave" -o carpeta.iron

# Restaurar
dotnet run --project src/IronShield.Cli -- unprotect archivo.iron -p "clave" -o restaurado
```

## Evolución futura: directorios estructurados (Caso B)

Actualmente `DirectoryDataSource` empaqueta el directorio como un ZIP en un solo `FileContent`. A futuro puede migrarse a bloques individuales por archivo sin romper compatibilidad:

| Paso | Cambio | Impacto |
|---|---|---|
| 1 | Agregar `IronBlockType.DirectoryEntry = 5` | Nuevo valor enum, no rompe existentes |
| 2 | Agregar `DirectoryEntry : IIronBlockData` con `RelativePath`, `Content`, `LastWriteUtc` | Nuevo modelo |
| 3 | Modificar `IronBlockDataFactory.Create()` para emitir N `DirectoryEntry` | Solo cambia la factoría |
| 4 | `IronUnprotectionService` reconstruye estructura desde entries | Nuevo case |

Los `.iron` legacy con ZIP embebido seguirán siendo legibles. Los nuevos con `DirectoryEntry` serán ignorados por lectores antiguos.

## Decisiones de arquitectura recientes

* CLI usa `System.CommandLine 2.0.9` (stable, API con `SetAction` + `ParseResult.GetValue<T>`).
* `CliOutputService.RunSafe()` captura `AuthenticationTagMismatchException` para mostrar "Incorrect password" sin stack trace.
* Las pruebas de CLI usan `[Collection("CLI")]` con paralelización desactivada por conflicto de `AnsiConsole.Status()`.
* `InternalsVisibleTo` desde `IronShield.Cli` hacia `IronShield.Cli.Tests`.
* Project reference desde CLI hacia `IronShield.Core`, `IronShield.Cryptography`, `IronShield.Storage`.

## Próximo objetivo técnico

* Comando `inspect` para leer metadatos públicos sin descifrar.
* CI/CD pipeline (GitHub Actions).
* Benchmark de rendimiento para archivos grandes.
