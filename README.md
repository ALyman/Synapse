Synapse Parser Combinator Framework
-----------------------------------

The Synapse Parser Combinator Framework is a token-based parser library that uses a simple, LINQ-based syntax to make writing parsers in C# simple.

    using Synapse;
    ...
    var input = new int[] { 1, 2 }.AsInput();
    var parser = from a in Parse.Match(1) // a == 1
    			 from b in Parse.Or(Parse.Match(2), Parse.Match(3)) // b == 2
    			 select a + b;
    var result = parser.Parse(input);
