# Vision and Goals

## Project Vision

IronShield busca convertirse en una solución moderna para el almacenamiento y transporte seguro de información sensible. El proyecto se desarrolla con una fuerte orientación a buenas prácticas de ingeniería de software y diseño de arquitectura.

## Long-Term Goals

* Protección local de archivos.
* Intercambio seguro de archivos mediante Internet.
* CLI multiplataforma.
* Interfaz gráfica basada en Avalonia UI.
* Posible incorporación futura de:

  * Firmas digitales.
  * Infraestructura de clave pública.
  * Auditoría y trazabilidad.
  * Integración con servicios externos.

## Design Philosophy

### El formato es el producto

El formato `.iron` es un componente central del proyecto. La implementación debe adaptarse al formato y no al contrario.

### Extensibilidad

Las nuevas características se incorporarán mediante nuevos bloques o nuevos modelos, evitando modificar el significado de los existentes.

### Forward Compatibility

Las versiones futuras deberán poder leer archivos antiguos. Los lectores deben ignorar bloques desconocidos.

### Separación de responsabilidades

* Modelos → representan datos.
* Servicios → ejecutan operaciones.
* Interfaces → definen contratos.
* Factorías → construyen objetos.
* Serializadores → transforman datos entre representaciones.

### Deuda técnica

Se evita introducir soluciones temporales que comprometan la evolución futura del proyecto.
