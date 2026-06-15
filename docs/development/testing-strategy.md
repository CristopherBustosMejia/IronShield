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