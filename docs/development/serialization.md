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

## JsonIronBlockSerializer

Implementación actual basada en `System.Text.Json`.

## Serialización polimórfica

IronShield no depende de la serialización automática de interfaces proporcionada por `System.Text.Json`.

Se utilizan convertidores personalizados para mantener el control del formato interno.

### Principios

* El discriminador pertenece al dominio.
* Se utiliza la propiedad `Algorithm`.
* No se introducen propiedades técnicas como `$type`.
* La representación JSON forma parte del diseño del proyecto y no de una biblioteca externa.

---
