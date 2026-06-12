# Cryptography Overview

## Objetivo

La capa criptográfica de IronShield proporciona mecanismos modernos y auditados para proteger la confidencialidad e integridad de la información almacenada en archivos `.iron`.

El proyecto no implementa algoritmos criptográficos propios; únicamente integra algoritmos estándar mediante APIs y bibliotecas confiables.

## Pipeline criptográfico

```text
Password
    ↓
Argon2id
    ↓
Derived Key
    ↓
AES-256-GCM
    ↓
EncryptedPayload
    ↓
IronContainer
```

## Algoritmos seleccionados

| Función              | Algoritmo   |
| -------------------- | ----------- |
| Integridad           | SHA-256     |
| Derivación de claves | Argon2id    |
| Cifrado              | AES-256-GCM |

## Principios

* Evitar criptografía personalizada.
* El archivo debe describir los algoritmos utilizados.
* Los parámetros criptográficos forman parte del formato.
* La arquitectura debe permitir incorporar nuevos algoritmos.
