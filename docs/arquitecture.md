# Architecture

## Estructura de la solución

```text
IronShield.sln
│
├── src
│   ├── IronShield.Core
│   ├── IronShield.Cryptography
│   ├── IronShield.Storage
│   └── IronShield.Cli
│
└── tests
    ├── IronShield.Core.Tests
    ├── IronShield.Cryptography.Tests
    ├── IronShield.Storage.Tests
    └── IronShield.Cli.Tests
```

## Responsabilidades

### IronShield.Core

Contiene el dominio del proyecto.

Responsabilidades:

* Modelos.
* Enumeraciones.
* Interfaces.
* Constantes.
* Contratos compartidos.

No contiene lógica de infraestructura.

### IronShield.Cryptography

Contiene todas las operaciones criptográficas.

Responsabilidades:

* Hashing.
* Derivación de claves.
* Cifrado y descifrado.
* Generación de material criptográfico.

### IronShield.Storage

Responsable del formato `.iron`, la obtención de datos y la orquestación del pipeline.

Responsabilidades:

* Lectura del contenedor.
* Escritura del contenedor.
* Serialización de bloques.
* Serialización binaria de bloques.
* Abstracción de fuentes de datos (`IDataSource`).
* Proveedores de origen: archivos, directorios (ZIP embebido), streams comprimidos.
* Factoría de bloques `IronBlockDataFactory` (lee fuente, construye metadata + contenido + integridad).
* Servicio de protección (`IronProtectionService`).
* Servicio de desprotección (`IronUnprotectionService`).
* Servicio de verificación de integridad (`IronIntegrityVerificationService`).
* Fachada unificada (`IronShieldService`) que los compone.

### IronShield.Cli

Interfaz de línea de comandos del proyecto.

### Proyectos de pruebas

Cada proyecto principal cuenta con su proyecto de pruebas correspondiente.

## Flujo general

### Protect (datos → .iron)

```text
Origen (archivo / directorio / stream)
        ↓
IDataSource (FileDataSource / DirectoryDataSource / CompressedDataSource)
        ↓
IDataCollector.Collect(source)
        ↓
IIronBlockData[] (hoy: PublicMetadata + FileContent + IntegrityData; futuro: + DirectoryEntry)
        ↓
IIronContainerFactory.Create(version, blocks, cryptographyContext)
        ↓
IronContainer
        ↓
IIronContainerWriter.Write(container, stream)
        ↓
Archivo .iron
```

### Unprotect (.iron → datos)

```text
Archivo .iron
        ↓
IIronContainerReader.Read(stream)
        ↓
IronContainer
        ↓
Extraer EncryptionInfo → derivar clave
        ↓
Descifrar y deserializar bloques
        ↓
UnprotectResult { Data, Metadata }
```

### Verify (.iron → integridad)

```text
Archivo .iron
        ↓
IIronContainerReader.Read(stream)
        ↓
Extraer EncryptionInfo → derivar clave
        ↓
Descifrar FileContent e IntegrityData
        ↓
Recomputar hash y comparar en tiempo constante
        ↓
IntegrityVerificationResult { IsAvailable, IsValid, HashAlgorithm }
```

La fachada `IronShieldService` encapsula los flujos delegando en `IronProtectionService` (Protect), `IronUnprotectionService` (Unprotect) e `IronIntegrityVerificationService` (Verify).

## Principios arquitectónicos

* Responsabilidades únicas.
* Bajo acoplamiento.
* Alta cohesión.
* Extensibilidad mediante composición.
* Evitar abstracciones prematuras.

## Evolución: directorios estructurados

Actualmente `DirectoryDataSource` empaqueta el directorio como un solo ZIP en `FileContent`. A futuro puede evolucionar a bloques `DirectoryEntry` individuales sin romper compatibilidad:

1. Nuevo `IronBlockType.DirectoryEntry = 5`
2. Nuevo modelo `DirectoryEntry : IIronBlockData` con `RelativePath`, `Content`, `LastWriteUtc`
3. `IronBlockDataFactory` produce N `DirectoryEntry` en vez de un ZIP
4. `IronUnprotectionService` reconstruye la estructura de directorios

Los archivos `.iron` existentes (ZIP en `FileContent`) siguen siendo legibles. Los nuevos archivos con `DirectoryEntry` son ignorados por lectores antiguos (forward compatibilidad).
