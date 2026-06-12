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
    └── IronShield.Storage.Tests
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

Responsable del formato `.iron`.

Responsabilidades:

* Lectura del contenedor.
* Escritura del contenedor.
* Serialización de bloques.
* Conversores JSON personalizados.

### IronShield.Cli

Interfaz de línea de comandos del proyecto.

### Proyectos de pruebas

Cada proyecto principal cuenta con su proyecto de pruebas correspondiente.

## Flujo general

```text
Archivo original
        ↓
Creación de modelos
        ↓
Serialización del bloque
        ↓
Construcción del IronContainer
        ↓
IronContainerWriter
        ↓
Archivo .iron
```

## Principios arquitectónicos

* Responsabilidades únicas.
* Bajo acoplamiento.
* Alta cohesión.
* Extensibilidad mediante composición.
* Evitar abstracciones prematuras.
