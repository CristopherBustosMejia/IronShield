# Block System

## Concepto

El formato `.iron` está compuesto por un contenedor principal (`IronContainer`) que agrupa una colección de bloques (`IronBlock`).

Cada bloque representa una unidad independiente de información.

## IronContainer

```csharp
public sealed class IronContainer
{
    public required byte Version { get; init; }
    public required IReadOnlyCollection<IronBlock> Blocks { get; init; }
}
```

### Responsabilidades

* Representar un archivo `.iron`.
* Almacenar la versión del formato.
* Agrupar todos los bloques.

## IronBlock

```csharp
public sealed class IronBlock
{
    public required IronBlockType Type { get; init; }
    public required bool IsEncrypted { get; init; }
    public required byte[] Data { get; init; }
}
```

### Responsabilidades

* Identificar el tipo de información almacenada.
* Indicar si el contenido requiere descifrado.
* Contener los datos serializados del bloque.

## Filosofía de diseño

* Cada bloque es autocontenido.
* Los bloques son independientes entre sí.
* Los lectores deben ignorar bloques desconocidos.
* Nuevas funcionalidades se agregan mediante nuevos bloques.
* La estructura existente nunca cambia de significado.

## IsEncrypted

La propiedad `IsEncrypted` permite decidir individualmente si un bloque será visible o requerirá contraseña.

Esto permite combinar información pública y privada dentro del mismo archivo.
