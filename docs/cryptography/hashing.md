# Hashing

## Objetivo

El subsistema de hashing permite verificar la integridad de la información protegida.

Actualmente se utiliza SHA-256.

## Modelo asociado

`IntegrityData`

Contiene:

* Nombre del algoritmo.
* Hash del contenido original.

## IHashProvider

Contrato responsable de calcular hashes a partir de una secuencia de bytes.

Las implementaciones concretas no deben conocer detalles del formato `.iron`.

## Sha256HashProvider

Implementación basada en `SHA256.HashData()` de .NET.

Características:

* Determinista.
* Produce siempre 32 bytes.
* Utilizado inicialmente para comprobación de integridad.

## Estrategia de pruebas

Se validan:

* Hashes conocidos.
* Determinismo.
* Diferencias ante entradas distintas.
* Longitud esperada del resultado.
