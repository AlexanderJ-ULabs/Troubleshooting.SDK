namespace Troubleshooting.Calculator.Module
{
    /// <summary>
    ///     This class needs to perform operations: ADD, SUB, MUL, DIV.
    ///     The service will receive two types of data: integers and operations.
    ///     You will receive this data and decide how to process it in a concurrent manner.
    ///     Each 2 integers that arrive must be paired to 1 operation,
    ///     using a "First In First Out" type logic, similar to a stack.
    ///     Result:     Post messages to the provider with the results of each operation.
    ///     Please include the whole operation in the message,
    ///     for example:    {Operation: ADD, Integers: [13,42], Result: 55}
    ///     Remember this is formatted for you; just create properties and values.
    /// </summary>
    internal class Processor
    {
    }
}