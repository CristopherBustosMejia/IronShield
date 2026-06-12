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

## Próximo objetivo técnico

Implementar el proveedor de cifrado basado en AES-GCM.

Este documento debe mantenerse actualizado después de cada decisión arquitectónica importante.