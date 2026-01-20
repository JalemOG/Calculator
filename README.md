#  Calculator – Proyecto 3  
**Algoritmos y Estructuras de Datos I**

Este proyecto implementa un **sistema cliente–servidor de cálculo de expresiones aritméticas**, desarrollado en **C# (.NET 9)**, aplicando conceptos fundamentales de algoritmos, estructuras de datos y arquitectura de software.

---

##  Arquitectura del Proyecto

La solución está organizada en **capas bien definidas**, lo que facilita el mantenimiento, la extensibilidad y las pruebas.

```
Calculator
│
├── src/
│   ├── Calculator.Core        # Lógica de cálculo (algoritmos y estructuras)
│   ├── Calculator.Server      # Servidor TCP
│   └── Calculator.Client      # Cliente gráfico (WinForms)
│
├── tests/
│   ├── Calculator.Core.Tests
│   ├── Calculator.Server.Tests
│   └── Calculator.Client.Tests
│
└── README.md
```

---

##  Descripción de los Componentes

### 🔹 Calculator.Core
Contiene toda la **lógica algorítmica del sistema**, independiente de red o interfaz gráfica.

Incluye:
- Tokenización de expresiones (`Tokenizer`)
- Conversión infix → postfix (algoritmo **Shunting Yard**)
- Construcción de árboles de expresión (`ExpressionTreeBuilder`)
- Evaluación de árboles (`ExpressionEvaluator`)
- Nodos del AST (`ValueNode`, `BinaryOperatorNode`, `UnaryOperatorNode`, etc.)

---

### 🔹 Calculator.Server
Servidor **TCP** que:
- Acepta múltiples clientes concurrentes
- Recibe expresiones aritméticas como texto
- Usa `Calculator.Core` para evaluarlas
- Devuelve el resultado o un mensaje de error
- Registra cada operación en archivos **CSV** por cliente

---

### 🔹 Calculator.Client
Cliente gráfico desarrollado con **WinForms**.

Funcionalidades:
- Conexión al servidor (host y puerto)
- Envío de expresiones aritméticas
- Historial visual de resultados
- Visualización de historiales (CSV) desde la GUI

---

##  Ejecución de Pruebas

```bash
dotnet test
```

---

##  Autor
**Janik Zúñiga Hamilton**  
Instituto Tecnológico de Costa Rica
