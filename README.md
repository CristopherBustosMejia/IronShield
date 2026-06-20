# IronShield Documentation

## Overview

IronShield es una herramienta multiplataforma desarrollada en C# y .NET con el objetivo de proteger archivos sensibles mediante criptografía moderna y un formato de archivo propio, extensible y preparado para evolucionar sin romper compatibilidad.

El proyecto está siendo desarrollado con una filosofía de arquitectura limpia, responsabilidades bien definidas y evitando deuda técnica temprana.

## Objetivos del proyecto

* Proteger archivos sensibles (`.env`, secretos, configuraciones, certificados, etc.).
* Compartir archivos de manera segura a través de Internet.
* Proporcionar una interfaz CLI moderna basada en `Spectre.Console.Cli`.
* Incorporar una interfaz gráfica con Avalonia UI en etapas posteriores.
* Mantener una arquitectura profesional apta para portafolio.

## Filosofía de diseño

* No existe una versión heredada que mantener.
* El formato actual es la base oficial del proyecto.
* Las nuevas características deben agregarse sin romper las anteriores.
* Los modelos representan datos.
* Los servicios realizan operaciones.
* Las factorías construyen objetos.
* No se crean capas o mappers innecesarios.
* Se prioriza la mantenibilidad sobre la velocidad de desarrollo.

## Estado actual

### Implementado

* Estructura de la solución.
* Modelos principales del dominio.
* Especificación del formato `.iron`.
* Sistema de bloques (`IronContainer` / `IronBlock`).
* Lectura y escritura del contenedor binario.
* Serialización JSON de bloques.
* Conversores JSON personalizados para tipos polimórficos.
* SHA-256.
* Argon2id.
* AES-256-GCM.
* Abstracción de fuentes de datos (`IDataSource`, `IDataCollector`).
* Proveedores de origen: archivo, directorio (ZIP), comprimido (GZip).
* Cobertura inicial de pruebas unitarias.

### En desarrollo

* Colector de datos integrado con flujo de cifrado.
* CLI (comandos `create`, `extract`, `info`, `verify`).
* Interfaz gráfica (Avalonia UI).

## Índice de documentación

* `vision-and-goals.md`
* `architecture.md`
* `project-context.md`
* `file-format/iron-file-format.md`
* `file-format/block-system.md`
* `cryptography/cryptography-overview.md`
* `cryptography/hashing.md`
* `cryptography/key-derivation.md`
* `development/serialization.md`
* `development/coding-guidelines.md`
* `development/testing-strategy.md`
* `roadmap.md`

---