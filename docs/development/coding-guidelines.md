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