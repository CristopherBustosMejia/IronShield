# Serialization

## Objetivo

Separar la representación en memoria de los modelos del formato binario persistido en el archivo `.iron`.

## IronContainer

El contenedor se serializa en formato binario utilizando:

* Magic bytes.
* Versión del formato.
* Número de bloques.
* Bloques serializados secuencialmente.

## IronBlock

Cada bloque contiene:

* Tipo.
* Indicador de cifrado.
* Longitud de los datos.
* Datos serializados.

## IIronBlockSerializer

Contrato encargado de convertir objetos del dominio a `byte[]` y viceversa.

## BinaryIronBlockSerializer

Implementación actual. Serializa los modelos directamente en binario con campos de longitud conocida y prefijos de longitud para cadenas y arreglos de bytes (little-endian, consistente con el formato del contenedor).

### Objetivo de diseño

El payload binario elimina el overhead del JSON (nombres de propiedades, signos de puntuación y codificación base64 de arreglos de bytes), reduciendo el peso del archivo `.iron` generado.

### Esquema por modelo

* `PublicMetadata`: nombre de archivo (cadena con prefijo de longitud), tamaño (int64), timestamp y offset (int64 + int64), autor (3 cadenas).
* `FileContent`: contenido crudo (int32 de longitud + bytes, sin base64).
* `IntegrityData`: algoritmo (cadena con prefijo de longitud) + hash (longitud + bytes).
* `EncryptionInfo`: algoritmo de cifrado (cadena) + parámetros de derivación de clave.
* `EncryptedPayload`: ciphertext (longitud + bytes) + colección de parámetros nombre/valor.

## Serialización polimórfica

IronShield no depende de la serialización automática de interfaces proporcionada por `System.Text.Json`.

Se utiliza un discriminador explícito para los parámetros de derivación de clave.

### Discriminador de derivación de clave

El primer byte del cuerpo de `EncryptionInfo` identifica los parámetros de derivación. Los códigos son estables y se agregan exclusivamente para nuevos algoritmos:

| Código | Algoritmo |
| ------ | --------- |
| 0x01   | Argon2id  |

Los lectores deben rechazar códigos desconocidos con `InvalidDataException`.