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

# File: docs/development/coding-guidelines.md

# Coding Guidelines

## Convenciones generales

* Utilizar `required` junto con `init` para modelos inmutables.
* Evitar `set` salvo necesidad justificada.
* Los modelos no contienen lógica de negocio.
* Los servicios no representan estado persistente.
* No crear DTOs o mappers redundantes.

## Organización

* Interfaces en `IronShield.Core/Interfaces`.
* Modelos en `IronShield.Core/Models`.
* Implementaciones concretas en el proyecto correspondiente.

## Validaciones

Utilizar preferentemente:

```csharp
ArgumentNullException.ThrowIfNull(value);
```

para parámetros públicos.

## Pruebas

Toda funcionalidad importante debe contar con pruebas unitarias antes de integrarse en la rama principal.

---

# File: docs/development/testing-strategy.md

# Testing Strategy

## Objetivo

Garantizar la estabilidad del formato `.iron` y del comportamiento de los componentes críticos del proyecto.

## Herramientas

* xUnit.
* FluentAssertions.

## Componentes a validar

* Modelos.
* Serialización.
* Lectura y escritura de contenedores.
* Algoritmos criptográficos.
* Conversores JSON personalizados.

## Principios

* Las pruebas deben ser independientes.
* Se utilizan vectores conocidos para criptografía.
* Los cambios arquitectónicos importantes deben acompañarse de nuevas pruebas.
* Antes de cada commit se recomienda ejecutar:

```bash
dotnet build
dotnet test
```