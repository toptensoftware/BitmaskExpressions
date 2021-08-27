# Bitmask Expressions

This is an experimental solution 

## The Problem

Suppose a set of bit flags like this:

```
enum
{
    Apples = 1 << 0,
    Pears = 1 << 1,
    Bananas = 1 << 2,
}
```

and suppose an expression like this:

```
Apples && (Pears || Bananas)
```

has been parsed into an abstract syntax tree like this:

``` 
AND
 |
 +-- Apples
 |
 +-- OR
      |
      +-- Pears
      |
      +-- Bananas
```

How to convert that AST into an optimal set of bitmask and test operations?

The only required expression operators are `AND`, `OR` and `NOT`.


## Sandbox

This project is a sandbox for experimenting with this problem and contains a
working solution.

The sandbox takes expressions with the following elements

* Letters A - Z map to bitmasks 0x0001, 0x0002, 0x0004 etc...
* Operators `&` (AND), `|` (OR) and `!` (NOT)
* Round parenthesis

Given an expression it will:

1. Parse into an AST
2. Log the AST to console
3. Create an optimized execution plan
4. Display the optimized execution plan as a C# like expression
5. Test the execution plan by executing over a range of integers 0-15 and 
   comparing to a non-optimized evaluation of the same expression
   

## Sample Output

```
Expression:

  A | (B & C)

AST:

  OR
    Identifier 0x01
    AND
      Identifier 0x02
      Identifier 0x04

Plan:

  ((input & 0x01) != 0x00) || ((input & 0x06) == 0x06)

Test: (expected vs actual)

  0000 => false vs false √
  0001 => TRUE  vs TRUE  √
  0010 => false vs false √
  0011 => TRUE  vs TRUE  √
  0100 => false vs false √
  0101 => TRUE  vs TRUE  √
  0110 => TRUE  vs TRUE  √
  0111 => TRUE  vs TRUE  √
  1000 => false vs false √
  1001 => TRUE  vs TRUE  √
  1010 => false vs false √
  1011 => TRUE  vs TRUE  √
  1100 => false vs false √
  1101 => TRUE  vs TRUE  √
  1110 => TRUE  vs TRUE  √
  1111 => TRUE  vs TRUE  √
```

## StackOverflow

This problem was originally posted as this StackOverflow question:

https://stackoverflow.com/questions/68949922/converting-a-boolean-expression-tree-to-a-series-of-bitwise-operations